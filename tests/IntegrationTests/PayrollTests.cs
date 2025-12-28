using System.Net;
using System.Text.RegularExpressions;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.EmployeeCenter.Entities;
using Microsoft.EntityFrameworkCore;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class PayrollTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public PayrollTests()
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
        await _server.UpdateDbAsync<TemplateDbContext>();
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
        // Use a simpler regex that doesn't depend on many quotes
        var match = Regex.Match(html, @"__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Could not find anti-CSRF token on page: {url}");
        }

        return match.Groups[1].Value;
    }

    [TestMethod]
    public async Task ManagePayrollTest()
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

        // 2. Create a normal user
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var userName = $"user-{uniqueId}";
        var email = $"{userName}@aiursoft.com";
        var password = "Test-Password-123";
        
        // Log off admin first to register
        await _http.GetAsync("/Account/LogOff");

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
        
        // 3. Get User ID from DB
        string userId;
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TemplateDbContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            Assert.IsNotNull(user);
            userId = user.Id;
        }

        // 4. Log in as admin again to issue payroll
        await _http.GetAsync("/Account/LogOff");
        loginToken = await GetAntiCsrfToken("/Account/Login");
        await _http.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", "admin" },
            { "Password", "admin123" },
            { "__RequestVerificationToken", loginToken }
        }));

        var createPayrollToken = await GetAntiCsrfToken("/Payroll/Create");
        var payrollContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "UserId", userId },
            { "TargetMonth", "2025-12" },
            { "TotalAmount", "5000" },
            { "Content", "# December Payroll\nBase: 4000\nBonus: 1000" },
            { "__RequestVerificationToken", createPayrollToken }
        });
        var createPayrollResponse = await _http.PostAsync("/Payroll/Create", payrollContent);
        Assert.AreEqual(HttpStatusCode.Found, createPayrollResponse.StatusCode);

        // 5. Verify payroll in Manage page
        var manageResponse = await _http.GetAsync("/Payroll/Manage");
        manageResponse.EnsureSuccessStatusCode();
        var manageHtml = await manageResponse.Content.ReadAsStringAsync();
        Assert.Contains(userName, manageHtml);

        // 6. Log in as the normal user and verify they can see their payroll
        await _http.GetAsync("/Account/LogOff");

        loginToken = await GetAntiCsrfToken("/Account/Login");
        loginResponse = await _http.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", email },
            { "Password", password },
            { "__RequestVerificationToken", loginToken }
        }));
        Assert.AreEqual(HttpStatusCode.Found, loginResponse.StatusCode);

        var myPayrollsResponse = await _http.GetAsync("/Payroll/Index");
        myPayrollsResponse.EnsureSuccessStatusCode();
        var myPayrollsHtml = await myPayrollsResponse.Content.ReadAsStringAsync();
        Assert.Contains("2025-12", myPayrollsHtml);
        
        // 7. View details
        int payrollId;
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TemplateDbContext>();
            var payroll = await db.Payrolls.FirstAsync(p => p.OwnerId == userId);
            payrollId = payroll.Id;
        }

        var detailsResponse = await _http.GetAsync("/Payroll/Details/" + payrollId);
        detailsResponse.EnsureSuccessStatusCode();
        var detailsHtml = await detailsResponse.Content.ReadAsStringAsync();
        Assert.Contains("December Payroll", detailsHtml);
    }
}
