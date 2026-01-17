
using Aiursoft.EmployeeCenter.Authorization;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class WeeklyReportTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public WeeklyReportTests()
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
        // Redirects to login are possible if not authenticated, but Register shouldn't.
        if (response.StatusCode == HttpStatusCode.Found) 
        {
             // Follow redirect once if needed, or assume caller handles it.
             // For Register/Login pages, usually 200.
        }
        
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html,
            @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");
        if (!match.Success)
        {
             // If we are redirected to login, maybe we are unauthorized.
             // But usually we call this on a page we expect to see form.
             // throw new InvalidOperationException($"Could not find anti-CSRF token on page: {url}. Status: {response.StatusCode}");
             // Return empty or let it fail later.
        }
        return match.Groups[1].Value;
    }

    private async Task<string> GetUserIdByEmail(string email)
    {
        using var scope = _server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
        var user = await db.Users.FirstAsync(u => u.Email == email);
        return user.Id;
    }

    private async Task GrantPermission(string userId, string permission)
    {
        using var scope = _server!.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var roleName = "TestRole_" + permission;
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var role = new IdentityRole(roleName);
            await roleManager.CreateAsync(role);
            await roleManager.AddClaimAsync(role, new Claim(AppPermissions.Type, permission));
        }

        var user = await userManager.FindByIdAsync(userId);
        await userManager.AddToRoleAsync(user!, roleName);
    }

    private async Task LoginAs(string username, string password)
    {
        await _http.GetAsync("/Account/LogOff");
        var loginToken = await GetAntiCsrfToken("/Account/Login");
        var loginContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", username },
            { "Password", password },
            { "__RequestVerificationToken", loginToken }
        });
        var loginResponse = await _http.PostAsync("/Account/Login", loginContent);
        if (loginResponse.StatusCode != HttpStatusCode.Found)
        {
             // Fallback: maybe already logged in or error?
             // But usually expects redirect to ReturnUrl or Home
        }
    }

    [TestMethod]
    public async Task WeeklyReportWorkflowTest()
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

        // Grant Permission
        var userId = await GetUserIdByEmail(email);
        await GrantPermission(userId, AppPermissionNames.CanCreateWeeklyReport);

        // Re-Login to refresh claims
        await LoginAs(email, password);

        // 2. Visit Weekly Report Index
        var indexResponse = await _http.GetAsync("/WeeklyReport/Index");
        Assert.AreEqual(HttpStatusCode.OK, indexResponse.StatusCode);
        var indexHtml = await indexResponse.Content.ReadAsStringAsync();
        
        // Should show "New Weekly Report"
        Assert.Contains("New Weekly Report", indexHtml);
        // Assert.IsFalse(indexHtml.Contains("New Weekly Report"));

        
        // 3. Submit Report for Current Week
        // We need to pick a valid Sunday. 
        // The form defaults to current week start.
        var now = DateTime.UtcNow;
        var offset = (int)now.DayOfWeek;
        var thisWeekStart = now.AddDays(-offset).Date;
        var weekStartStr = thisWeekStart.ToString("yyyy-MM-dd");

        var createToken = await GetAntiCsrfToken("/WeeklyReport/Index");
        var createContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "content", "My first report" },
            { "weekStartDate", weekStartStr },
            { "__RequestVerificationToken", createToken }
        });

        var createResponse = await _http.PostAsync("/WeeklyReport/Create", createContent);
        Assert.AreEqual(HttpStatusCode.Found, createResponse.StatusCode); // Redirect to Index

        // 4. Verify in DB
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var report = await db.WeeklyReports.FirstOrDefaultAsync(r => r.UserId == userId);
            Assert.IsNotNull(report);
            Assert.AreEqual("My first report", report.Content);
            Assert.AreEqual(thisWeekStart, report.WeekStartDate);
        }

        // 5. Try to submit AGAIN for same week
        // Note: The UI might hide it, but we can POST manually to test backend validation.
        createContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "content", "Duplicate report attempt" },
            { "weekStartDate", weekStartStr },
            { "__RequestVerificationToken", createToken }
        });
        createResponse = await _http.PostAsync("/WeeklyReport/Create", createContent);
        Assert.AreEqual(HttpStatusCode.Found, createResponse.StatusCode); 
        // Logic says: if exists, return RedirectToAction(nameof(Index));
        // So we get a redirect. But we should check DB that content didn't change or new report wasn't added.

        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var reports = await db.WeeklyReports.Where(r => r.UserId == userId).ToListAsync();
            Assert.HasCount(1, reports); // Still 1
            Assert.AreEqual("My first report", reports[0].Content);
        }

        // 6. Submit for Past Week
        var pastWeekStart = thisWeekStart.AddDays(-7);
        var pastWeekStr = pastWeekStart.ToString("yyyy-MM-dd");

        createContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "content", "Past report" },
            { "weekStartDate", pastWeekStr },
            { "__RequestVerificationToken", createToken }
        });
        createResponse = await _http.PostAsync("/WeeklyReport/Create", createContent);
        Assert.AreEqual(HttpStatusCode.Found, createResponse.StatusCode);

        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var reports = await db.WeeklyReports.Where(r => r.UserId == userId).OrderBy(r => r.WeekStartDate).ToListAsync();
            Assert.HasCount(2, reports);
            Assert.AreEqual(pastWeekStart, reports[0].WeekStartDate);
            Assert.AreEqual("Past report", reports[0].Content);
            Assert.AreEqual(thisWeekStart, reports[1].WeekStartDate);
        }
    }

    [TestMethod]
    public async Task TestEditWeeklyReport()
    {
        var email = $"test-edit-{Guid.NewGuid()}@aiursoft.com";
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
        var userId = await GetUserIdByEmail(email);
        await GrantPermission(userId, AppPermissionNames.CanCreateWeeklyReport);
        await LoginAs(email, password);

        // 2. Submit a report
        var createToken = await GetAntiCsrfToken("/WeeklyReport/Index");
        var createContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "content", "Original Content" },
            { "weekStartDate", DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek).ToString("yyyy-MM-dd") },
            { "__RequestVerificationToken", createToken }
        });
        await _http.PostAsync("/WeeklyReport/Create", createContent);

        int reportId;
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var report = await db.WeeklyReports.FirstAsync(r => r.UserId == userId);
            reportId = report.Id;
        }

        // 3. Edit the report
        var editToken = await GetAntiCsrfToken($"/WeeklyReport/Edit/{reportId}");
        var editContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Id", reportId.ToString() },
            { "Content", "Updated Content" },
            { "__RequestVerificationToken", editToken }
        });
        var editResponse = await _http.PostAsync("/WeeklyReport/Edit", editContent);
        Assert.AreEqual(HttpStatusCode.Found, editResponse.StatusCode);

        // 4. Verify in DB
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var report = await db.WeeklyReports.FirstAsync(r => r.Id == reportId);
            Assert.AreEqual("Updated Content", report.Content);
        }

        // 5. Try to edit someone else's report
        var otherEmail = $"other-{Guid.NewGuid()}@aiursoft.com";
        var registerTokenOther = await GetAntiCsrfToken("/Account/Register");
        var registerContentOther = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email", otherEmail },
            { "Password", password },
            { "ConfirmPassword", password },
            { "__RequestVerificationToken", registerTokenOther }
        });
        await _http.PostAsync("/Account/Register", registerContentOther);
        
        var otherId = await GetUserIdByEmail(otherEmail);
        await GrantPermission(otherId, AppPermissionNames.CanCreateWeeklyReport); // Grant permission so we can get token from Index
        
        await LoginAs(otherEmail, password);

        var editGetResponseOther = await _http.GetAsync($"/WeeklyReport/Edit/{reportId}");
        Assert.AreEqual(HttpStatusCode.Unauthorized, editGetResponseOther.StatusCode);

        var tokenForOther = await GetAntiCsrfToken("/WeeklyReport/Index");
        var editContentOther = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Id", reportId.ToString() },
            { "Content", "Try Update Other's Content" },
            { "__RequestVerificationToken", tokenForOther }
        });
        var editResponseOther = await _http.PostAsync("/WeeklyReport/Edit", editContentOther);
        Assert.AreEqual(HttpStatusCode.Unauthorized, editResponseOther.StatusCode);

        // 6. Try to edit a report older than 4 weeks
        await LoginAs(email, password);
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var oldReport = new WeeklyReport
            {
                UserId = userId,
                Content = "Old Content",
                CreateTime = DateTime.UtcNow.AddDays(-30),
                WeekStartDate = DateTime.UtcNow.AddDays(-35)
            };
            db.WeeklyReports.Add(oldReport);
            await db.SaveChangesAsync();
            reportId = oldReport.Id;
        }

        var editTokenOld = await GetAntiCsrfToken($"/WeeklyReport/Edit/{reportId}");
        var oldEditContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Id", reportId.ToString() },
            { "Content", "Try Update Old Content" },
            { "__RequestVerificationToken", editTokenOld }
        });
        var oldEditResponse = await _http.PostAsync("/WeeklyReport/Edit", oldEditContent);
        Assert.AreEqual(HttpStatusCode.BadRequest, oldEditResponse.StatusCode);
    }
}
