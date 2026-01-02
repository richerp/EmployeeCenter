using System.Net;
using System.Text.RegularExpressions;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.EmployeeCenter.Entities;
using Microsoft.AspNetCore.Identity;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class ReportLineTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public ReportLineTests()
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
        var match = Regex.Match(html,
            @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Could not find anti-CSRF token on page: {url}");
        }

        return match.Groups[1].Value;
    }

    private async Task LoginAsync(string email, string password)
    {
        var loginToken = await GetAntiCsrfToken("/Account/Login");
        var loginContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", email },
            { "Password", password },
            { "__RequestVerificationToken", loginToken }
        });
        var loginResponse = await _http.PostAsync("/Account/Login", loginContent);
        Assert.AreEqual(HttpStatusCode.Found, loginResponse.StatusCode);
    }

    private async Task LogOffAsync()
    {
        var logOffToken = await GetAntiCsrfToken("/Manage/ChangePassword");
        var logOffContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "__RequestVerificationToken", logOffToken }
        });
        await _http.PostAsync("/Account/LogOff", logOffContent);
    }

    [TestMethod]
    public async Task TestReportLineFlow()
    {
        // 1. Register User A
        var userAEmail = "usera@test.com";
        var userAPassword = "Password123!";
        var registerToken = await GetAntiCsrfToken("/Account/Register");
        var registerContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email", userAEmail },
            { "Password", userAPassword },
            { "ConfirmPassword", userAPassword },
            { "__RequestVerificationToken", registerToken }
        });
        await _http.PostAsync("/Account/Register", registerContent);
        
        // Get User A ID
        await _http.GetAsync("/Manage/Index");
        // Since we don't have ID on manage page easily, let's just use admin to find it.
        
        await LogOffAsync();

        // 2. Register User B
        var userBEmail = "userb@test.com";
        var userBPassword = "Password123!";
        registerToken = await GetAntiCsrfToken("/Account/Register");
        registerContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email", userBEmail },
            { "Password", userBPassword },
            { "ConfirmPassword", userBPassword },
            { "__RequestVerificationToken", registerToken }
        });
        await _http.PostAsync("/Account/Register", registerContent);
        await LogOffAsync();

        // 3. Login as Admin and set B as A's manager
        await LoginAsync("admin@default.com", "admin123");
        
        // Find IDs
        using var scope = _server!.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var userA = await userManager.FindByEmailAsync(userAEmail);
        var userB = await userManager.FindByEmailAsync(userBEmail);
        Assert.IsNotNull(userA);
        Assert.IsNotNull(userB);

        var editToken = await GetAntiCsrfToken($"/Users/Edit/{userA.Id}");
        var editContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Id", userA.Id },
            { "UserName", userA.UserName! },
            { "DisplayName", userA.DisplayName },
            { "Email", userA.Email! },
            { "AvatarUrl", userA.AvatarRelativePath },
            { "ManagerId", userB.Id },
            { "ManagerDisplayName", userB.DisplayName },
            { "__RequestVerificationToken", editToken }
        });
        var editResponse = await _http.PostAsync($"/Users/Edit/{userA.Id}", editContent);
        Assert.AreEqual(HttpStatusCode.Found, editResponse.StatusCode);

        // 4. Login as User A and view report line
        await LogOffAsync();
        await LoginAsync(userAEmail, userAPassword);
        
        var reportLinePage = await _http.GetAsync("/ReportLine");
        reportLinePage.EnsureSuccessStatusCode();
        var reportLineHtml = await reportLinePage.Content.ReadAsStringAsync();
        Assert.Contains(userB.DisplayName, reportLineHtml);
        Assert.Contains("Management Chain", reportLineHtml);

        // 5. Login as Admin and view B's report line (should see A as subordinate)
        await LogOffAsync();
        await LoginAsync("admin@default.com", "admin123");
        
        reportLinePage = await _http.GetAsync($"/ReportLine/Index/{userB.Id}");
        reportLineHtml = await reportLinePage.Content.ReadAsStringAsync();
        Assert.Contains(userA.DisplayName, reportLineHtml);
        Assert.Contains("Direct Reports", reportLineHtml);
        
        await LogOffAsync();
        
        // 6. Login as User A and try to view B's report line (should fail)
        await LoginAsync(userAEmail, userAPassword);
        var otherReportLine = await _http.GetAsync($"/ReportLine/Index/{userB.Id}");
        Assert.AreEqual(HttpStatusCode.Unauthorized, otherReportLine.StatusCode);
    }

    [TestMethod]
    public async Task TestCircularDependency()
    {
        // 1. Create two users
        string idA, idB, emailA;
        var suffix = Guid.NewGuid().ToString("N")[..6];
        using (var scope = _server!.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var userA = new User { UserName = "usera" + suffix, DisplayName = "User A", Email = "usera" + suffix + "@test.com", AvatarRelativePath = User.DefaultAvatarPath };
            var userB = new User { UserName = "userb" + suffix, DisplayName = "User B", Email = "userb" + suffix + "@test.com", AvatarRelativePath = User.DefaultAvatarPath };
            var resA = await userManager.CreateAsync(userA, "Password123!");
            var resB = await userManager.CreateAsync(userB, "Password123!");
            if (!resA.Succeeded || !resB.Succeeded)
            {
                throw new Exception("Failed to create users: " + string.Join(", ", resA.Errors.Concat(resB.Errors).Select(e => e.Description)));
            }
            idA = userA.Id;
            idB = userB.Id;
            emailA = userA.Email;
        }

        // 2. Set A's manager to B, and B's manager to A
        using (var scope = _server!.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var dbUserA = await userManager.FindByIdAsync(idA);
            var dbUserB = await userManager.FindByIdAsync(idB);
            dbUserA!.ManagerId = idB;
            dbUserB!.ManagerId = idA;
            await userManager.UpdateAsync(dbUserA);
            await userManager.UpdateAsync(dbUserB);
        }
        
        // 3. Login as User A and view report line
        await LoginAsync(emailA, "Password123!");
        
        var reportLinePage = await _http.GetAsync("/ReportLine");
        reportLinePage.EnsureSuccessStatusCode();
        var reportLineHtml = await reportLinePage.Content.ReadAsStringAsync();
        
        Assert.Contains("Circular dependency detected in report line!", reportLineHtml);
    }
}
