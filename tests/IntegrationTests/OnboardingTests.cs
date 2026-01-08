using System.Net;
using System.Text.RegularExpressions;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.EmployeeCenter.Entities;
using Microsoft.EntityFrameworkCore;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class OnboardingTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public OnboardingTests()
    {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            AllowAutoRedirect = false
        };
        _port = Network.GetAvailablePort();
        _http = new HttpClient(handler)
        {
            BaseAddress = new Uri($"http://localhost:{_port}")
        };
    }

    [TestInitialize]
    public async Task CreateServer()
    {
        _server = await AppAsync<Startup>([], port: _port);
        await _server.UpdateDbAsync<EmployeeCenterDbContext>();
        await _server.SeedAsync();
        await _server.StartAsync();
    }

    [TestCleanup]
    public async Task CleanServer()
    {
        if (_server == null) return;
        await _server.StopAsync();
        _server.Dispose();
    }

    private async Task<string> GetAntiCsrfToken(string url)
    {
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html, @"__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Could not find anti-CSRF token on page: {url}");
        }

        return match.Groups[1].Value;
    }

    [TestMethod]
    public async Task OnboardingWorkflowTest()
    {
        // 1. Login as admin
        var loginToken = await GetAntiCsrfToken("/Account/Login");
        var loginContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", "admin" },
            { "Password", "admin123" },
            { "__RequestVerificationToken", loginToken }
        });
        var loginResponse = await _http.PostAsync("/Account/Login", loginContent);
        Assert.AreEqual(HttpStatusCode.Found, loginResponse.StatusCode);

        // 2. Create an onboarding task
        var createTaskToken = await GetAntiCsrfToken("/ManageOnboarding/Create");
        var taskContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Order", "1" },
            { "ExpectedDurationSeconds", "60" },
            { "Title", "Welcome Task" },
            { "Description", "Please read the manual." },
            { "StartLink", "https://example.com" },
            { "__RequestVerificationToken", createTaskToken }
        });
        var createTaskResponse = await _http.PostAsync("/ManageOnboarding/Create", taskContent);
        Assert.AreEqual(HttpStatusCode.Found, createTaskResponse.StatusCode);

        // 3. Verify task in Manage page
        var manageResponse = await _http.GetAsync("/ManageOnboarding/Index");
        manageResponse.EnsureSuccessStatusCode();
        var manageHtml = await manageResponse.Content.ReadAsStringAsync();
        Assert.Contains("Welcome Task", manageHtml);

        // 4. Create a normal user
        await _http.GetAsync("/Account/LogOff");
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var userName = $"user-{uniqueId}";
        var email = $"{userName}@aiursoft.com";
        var password = "Test-Password-123";

        var registerToken = await GetAntiCsrfToken("/Account/Register");
        var registerContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email", email },
            { "Password", password },
            { "ConfirmPassword", password },
            { "__RequestVerificationToken", registerToken }
        });
        var registerResponse = await _http.PostAsync("/Account/Register", registerContent);
        Assert.AreEqual(HttpStatusCode.Found, registerResponse.StatusCode);

        // 5. Login as the normal user
        loginToken = await GetAntiCsrfToken("/Account/Login");
        loginResponse = await _http.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", email },
            { "Password", password },
            { "__RequestVerificationToken", loginToken }
        }));
        Assert.AreEqual(HttpStatusCode.Found, loginResponse.StatusCode);

        // 6. Verify task on Dashboard
        var dashboardResponse = await _http.GetAsync("/Dashboard");
        dashboardResponse.EnsureSuccessStatusCode();
        var dashboardHtml = await dashboardResponse.Content.ReadAsStringAsync();
        Assert.Contains("Welcome Task", dashboardHtml);
        Assert.Contains("Start Task", dashboardHtml);

        // 7. Start the task
        int taskId;
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var task = await db.OnboardingTasks.FirstAsync(t => t.Title == "Welcome Task");
            taskId = task.Id;
        }

        var startTaskToken = await GetAntiCsrfToken("/Dashboard"); 
        var startTaskContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "__RequestVerificationToken", startTaskToken }
        });
        var startTaskResponse = await _http.PostAsync($"/Onboarding/StartTask/{taskId}", startTaskContent);
        Assert.AreEqual(HttpStatusCode.Found, startTaskResponse.StatusCode);
        Assert.AreEqual("https://example.com/", startTaskResponse.Headers.Location?.ToString());

        // 8. Verify task is in progress on Dashboard
        dashboardResponse = await _http.GetAsync("/Dashboard");
        dashboardResponse.EnsureSuccessStatusCode();
        dashboardHtml = await dashboardResponse.Content.ReadAsStringAsync();
        Assert.Contains("In Progress", dashboardHtml);

        // 9. Try to complete the task immediately
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var log = await db.OnboardingTaskLogs.FirstAsync(l => l.TaskId == taskId && l.CompletionTime == null);
            log.StartTime = DateTime.UtcNow.AddSeconds(-31);
            await db.SaveChangesAsync();
        }

        // 10. Complete the task
        var completeTaskToken = await GetAntiCsrfToken("/Dashboard");
        var completeTaskContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "__RequestVerificationToken", completeTaskToken }
        });
        var completeTaskResponse = await _http.PostAsync($"/Onboarding/CompleteTask/{taskId}", completeTaskContent);
        Assert.AreEqual(HttpStatusCode.Found, completeTaskResponse.StatusCode);

        // 11. Verify task is completed on Dashboard
        dashboardResponse = await _http.GetAsync("/Dashboard");
        dashboardResponse.EnsureSuccessStatusCode();
        dashboardHtml = await dashboardResponse.Content.ReadAsStringAsync();
        Assert.Contains("Completed at", dashboardHtml);
        Assert.Contains("You have completed all onboarding tasks", dashboardHtml);
        Assert.Contains("Certificate", dashboardHtml);
        Assert.Contains("Wish your career takes off at Aiursoft!", dashboardHtml);
    }
}
