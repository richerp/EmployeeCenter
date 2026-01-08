using System.Net;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.EmployeeCenter.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

using static Aiursoft.WebTools.Extends;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class TeamCalendarTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public TeamCalendarTests()
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

    private async Task<string> GetAntiCsrfToken(string url, HttpClient? client = null)
    {
        client ??= _http;
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html,
            @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    [TestMethod]
    public async Task TeamCalendarViewTest()
    {
        // 1. Create a Manager and a Subordinate
        string managerEmail = "manager@aiursoft.com";
        string subordinateEmail = "subordinate@aiursoft.com";
        string password = "Test-Password-123";

        // Clients
        var managerHttp = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer(), AllowAutoRedirect = false }) { BaseAddress = new Uri($"http://localhost:{_port}") };
        var subordinateHttp = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer(), AllowAutoRedirect = false }) { BaseAddress = new Uri($"http://localhost:{_port}") };

        // Register Manager
        var registerToken = await GetAntiCsrfToken("/Account/Register", managerHttp);
        var response = await managerHttp.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email", managerEmail },
            { "Password", password },
            { "ConfirmPassword", password },
            { "__RequestVerificationToken", registerToken }
        }));
        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);

        // Get Manager ID
        string managerId;
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var manager = await db.Users.FirstAsync(u => u.Email == managerEmail);
            managerId = manager.Id;
        }

        // Register Subordinate
        registerToken = await GetAntiCsrfToken("/Account/Register", subordinateHttp);
        response = await subordinateHttp.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email", subordinateEmail },
            { "Password", password },
            { "ConfirmPassword", password },
            { "__RequestVerificationToken", registerToken }
        }));
        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);

        // Set Manager for Subordinate
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var subordinate = await db.Users.FirstAsync(u => u.Email == subordinateEmail);
            subordinate.ManagerId = managerId;
            await db.SaveChangesAsync();
        }

        // 2. Check Subordinate's Team Calendar
        response = await subordinateHttp.GetAsync("/Leave/TeamCalendar");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        
        // Should see "Boss" or "经理"
        Assert.IsTrue(html.Contains("Boss") || html.Contains("经理") || html.Contains("汇报对象"));
        Assert.IsTrue(html.Contains(managerEmail));

        // 3. Check Manager's Team Calendar
        response = await managerHttp.GetAsync("/Leave/TeamCalendar");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        html = await response.Content.ReadAsStringAsync();

        // Should see "Direct Report" or "直接下属"
        Assert.IsTrue(html.Contains("Direct Report") || html.Contains("直接下属"));
        Assert.IsTrue(html.Contains(subordinateEmail));
    }
}
