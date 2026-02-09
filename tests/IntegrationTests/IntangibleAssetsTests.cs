namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class IntangibleAssetTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public IntangibleAssetTests()
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
    public async Task IntangibleAssetLifecycleTest()
    {
        // 1. Login as admin
        var loginToken = await GetAntiCsrfToken("/Account/Login");
        var loginContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", "admin" },
            { "Password", "admin123" },
            { "__RequestVerificationToken", loginToken }
        });
        await _http.PostAsync("/Account/Login", loginContent);

        // 2. Create Intangible Asset
        var createToken = await GetAntiCsrfToken("/IntangibleAssets/Create");
        var assetName = "Test Domain";
        await _http.PostAsync("/IntangibleAssets/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Name", assetName },
            { "Type", ((int)IntangibleAssetType.Domain).ToString() },
            { "Status", ((int)IntangibleAssetStatus.Active).ToString() },
            { "Supplier", "GoDaddy" },
            { "__RequestVerificationToken", createToken }
        }));

        Guid assetId;
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var asset = await db.IntangibleAssets.FirstAsync(a => a.Name == assetName);
            assetId = asset.Id;
            Assert.AreEqual(IntangibleAssetType.Domain, asset.Type);
            Assert.AreEqual(IntangibleAssetStatus.Active, asset.Status);
        }

        // 3. Edit Intangible Asset
        var editToken = await GetAntiCsrfToken($"/IntangibleAssets/Edit/{assetId}");
        await _http.PostAsync("/IntangibleAssets/Edit", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Id", assetId.ToString() },
            { "Name", "Updated Domain" },
            { "Type", ((int)IntangibleAssetType.Domain).ToString() },
            { "Status", ((int)IntangibleAssetStatus.Running).ToString() },
            { "Supplier", "NameCheap" },
            { "__RequestVerificationToken", editToken }
        }));

        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var asset = await db.IntangibleAssets.FirstAsync(a => a.Id == assetId);
            Assert.AreEqual("Updated Domain", asset.Name);
            Assert.AreEqual(IntangibleAssetStatus.Running, asset.Status);
            Assert.AreEqual("NameCheap", asset.Supplier);
        }

        // 4. Assign Intangible Asset
        // Create a user first
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var userName = $"user-{uniqueId}";
        var email = $"{userName}@aiursoft.com";
        var password = "Test-Password-123";

        await _http.GetAsync("/Account/LogOff");
        var registerToken = await GetAntiCsrfToken("/Account/Register");
        await _http.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email", email },
            { "Password", password },
            { "ConfirmPassword", password },
            { "__RequestVerificationToken", registerToken }
        }));

        string userId;
        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var user = await db.Users.FirstAsync(u => u.UserName == userName);
            userId = user.Id;
        }

        // Login as admin again
        await _http.GetAsync("/Account/LogOff");
        loginToken = await GetAntiCsrfToken("/Account/Login");
        await _http.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", "admin" },
            { "Password", "admin123" },
            { "__RequestVerificationToken", loginToken }
        }));

        var assignToken = await GetAntiCsrfToken($"/IntangibleAssets/Assign/{assetId}");
        await _http.PostAsync("/IntangibleAssets/Assign", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "AssetId", assetId.ToString() },
            { "AssigneeId", userId },
            { "__RequestVerificationToken", assignToken }
        }));

        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var asset = await db.IntangibleAssets.FirstAsync(a => a.Id == assetId);
            Assert.AreEqual(userId, asset.AssigneeId);
        }

        // 5. Delete Intangible Asset
        var deleteToken = await GetAntiCsrfToken($"/IntangibleAssets/Edit/{assetId}"); // Delete form is in Edit page
        await _http.PostAsync($"/IntangibleAssets/Delete/{assetId}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "__RequestVerificationToken", deleteToken }
        }));

        using (var scope = _server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var exists = await db.IntangibleAssets.AnyAsync(a => a.Id == assetId);
            Assert.IsFalse(exists);
        }
    }
}
