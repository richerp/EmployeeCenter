using System.Net;
using System.Text.RegularExpressions;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.EmployeeCenter.Entities;
using Microsoft.EntityFrameworkCore;

using static Aiursoft.WebTools.Extends;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class LeaveTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public LeaveTests()
    {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            AllowAutoRedirect = false // We need to handle redirects manually to check locations
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



    private async Task<int> GetLatestLeaveId(string userId)
    {
        using var scope = _server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
        var leave = await db.LeaveApplications
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Id)
            .FirstOrDefaultAsync();
        return leave?.Id ?? 0;
    }



    // Helper to get user ID based on email
    private async Task<string> GetUserIdByEmail(string email)
    {
        using var scope = _server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
        var user = await db.Users.FirstAsync(u => u.Email == email);
        return user.Id;
    }

    [TestMethod]
    public async Task LeaveWithdrawalTest()
    {
        var email = $"test-{Guid.NewGuid()}@aiursoft.com";
        var password = "Test-Password-123";

        // 1. Register and Login
        var registerToken = await GetAntiCsrfToken("/Account/Register");
        var registerContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email", email },
            { "Password", password },
            { "ConfirmPassword", password },
            { "__RequestVerificationToken", registerToken }
        });
        await _http.PostAsync("/Account/Register", registerContent);

        // 2. Initialize Allocation (Visit Index)
        await _http.GetAsync("/Leave/Index");

        // 3. Apply for FUTURE leave (Tomorrow)
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var applyToken = await GetAntiCsrfToken("/Leave/Apply");
        var applyContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "LeaveType", "AnnualLeave" },
            { "StartDate", tomorrow.ToString("yyyy-MM-dd") },
            { "EndDate", tomorrow.ToString("yyyy-MM-dd") },
            { "Reason", "Vacation" },
            { "__RequestVerificationToken", applyToken }
        });
        var applyResponse = await _http.PostAsync("/Leave/Apply", applyContent);
        Assert.AreEqual(HttpStatusCode.Found, applyResponse.StatusCode);

        // Get Leave ID
        var userId = await GetUserIdByEmail(email);
        var leaveId = await GetLatestLeaveId(userId);
        Assert.AreNotEqual(0, leaveId);

        // 4. Withdraw validity Check (Date < StartDate)
        // Today < Tomorrow => OK.

        // 5. Withdraw
        // Note: Withdraw is POST /Leave/Withdraw/{id} ? No, usually POST form data or query string?
        // Controller: public async Task<IActionResult> Withdraw(int id)
        // In ASP.NET Core MVC, if parameter is 'id', it can come from Route or Form or Query.
        // The form action is asp-route-id="@item.Id", so it generates /Leave/Withdraw/123
        // And method is POST.

        // Need token for withdraw form? Yes, ValidateAntiForgeryToken is on.
        // I need to get token from Index page which has the form?
        // Or just get any token from Leave/Index.
        var withdrawToken = await GetAntiCsrfToken("/Leave/Index");

        // The form submits to /Leave/Withdraw/{id}
        var withdrawResponse = await _http.PostAsync($"/Leave/Withdraw/{leaveId}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "__RequestVerificationToken", withdrawToken }
        }));

        Assert.AreEqual(HttpStatusCode.Found, withdrawResponse.StatusCode); // Redirect to Index

        // 6. Verify IsWithdrawn
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var leave = await db.LeaveApplications.FindAsync(leaveId);
            Assert.IsTrue(leave!.IsWithdrawn);
        }
    }

    [TestMethod]
    public async Task CannotWithdrawStartedLeaveTest()
    {
        var email = $"test-{Guid.NewGuid()}@aiursoft.com";
        var password = "Test-Password-123";

        // 1. Register
        var registerToken = await GetAntiCsrfToken("/Account/Register");
        var registerContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
             { "Email", email },
             { "Password", password },
             { "ConfirmPassword", password },
             { "__RequestVerificationToken", registerToken }
        });
        await _http.PostAsync("/Account/Register", registerContent);

        // 2. Visit Index to init
        await _http.GetAsync("/Leave/Index");

        // 3. Apply for TODAY leave (Already started)
        var today = DateTime.UtcNow.Date;
        var applyToken = await GetAntiCsrfToken("/Leave/Apply");
        var applyContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "LeaveType", "AnnualLeave" },
            { "StartDate", today.ToString("yyyy-MM-dd") },
            { "EndDate", today.ToString("yyyy-MM-dd") },
            { "Reason", "Sick" },
            { "__RequestVerificationToken", applyToken }
        });
        var applyResponse = await _http.PostAsync("/Leave/Apply", applyContent);
        Assert.AreEqual(HttpStatusCode.Found, applyResponse.StatusCode);

        // Get Leave ID
        var userId = await GetUserIdByEmail(email);
        var leaveId = await GetLatestLeaveId(userId);

        // 4. Attempt Withdraw
        // Since the leave has started, the "Withdraw" button (and its form) is NOT rendered on Index.
        // So we cannot get the token from Index page. We must get it from another page, e.g. Apply.
        var withdrawToken = await GetAntiCsrfToken("/Leave/Apply");
        var withdrawResponse = await _http.PostAsync($"/Leave/Withdraw/{leaveId}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "__RequestVerificationToken", withdrawToken }
        }));

        // 5. Assert Failure
        // Controller returns BadRequest("Cannot withdraw...") -> 400
        Assert.AreEqual(HttpStatusCode.BadRequest, withdrawResponse.StatusCode);

        // 6. Verify DB Not Withdrawn
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var leave = await db.LeaveApplications.FindAsync(leaveId);
            Assert.IsFalse(leave!.IsWithdrawn);
        }
    }
}
