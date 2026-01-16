
namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class ContractTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public ContractTests()
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
        // Use a simpler regex that doesn't depend on many quotes
        var match = Regex.Match(html, @"__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Could not find anti-CSRF token on page: {url}");
        }

        return match.Groups[1].Value;
    }

    [TestMethod]
    public async Task ManageContractTest()
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

        // 2. Create a PUBLIC contract
        // First upload the file via vault framework to get the logical path
        var createPublicContractToken = await GetAntiCsrfToken("/ManageContract/Create");

        string publicFilePath;
        using (var uploadContent = new MultipartFormDataContent())
        {
            var fileContent = new ByteArrayContent([1, 2, 3]);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            uploadContent.Add(fileContent, "file", "policy.pdf");

            var uploadResponse = await _http.PostAsync("/upload/contract?useVault=true", uploadContent);
            uploadResponse.EnsureSuccessStatusCode();
            var uploadResult = await uploadResponse.Content.ReadAsStringAsync();
            // Extract the Path from JSON response: {"Path":"contract/2026/01/15/policy.pdf","InternetPath":"..."}
            var pathMatch = Regex.Match(uploadResult, @"""Path"":""([^""]+)""");
            Assert.IsTrue(pathMatch.Success, "Failed to extract file path from upload response");
            publicFilePath = pathMatch.Groups[1].Value;
        }

        // Now submit the form with the logical path
        var createPublicContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Name", "Public Company Policy" },
            { "FilePath", publicFilePath },
            { "Status", "1" }, // Active
            { "IsPublic", "true" },
            { "__RequestVerificationToken", createPublicContractToken }
        });
        var createContractResponse = await _http.PostAsync("/ManageContract/Create", createPublicContent);
        Assert.AreEqual(HttpStatusCode.Found, createContractResponse.StatusCode);

        // 3. Create a PRIVATE contract
        // First upload the file via vault framework to get the logical path
        var createPrivateContractToken = await GetAntiCsrfToken("/ManageContract/Create");

        string privateFilePath;
        using (var uploadContent = new MultipartFormDataContent())
        {
            var fileContent = new ByteArrayContent([1, 2, 3]);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            uploadContent.Add(fileContent, "file", "secret.pdf");

            var uploadResponse = await _http.PostAsync("/upload/contract?useVault=true", uploadContent);
            uploadResponse.EnsureSuccessStatusCode();
            var uploadResult = await uploadResponse.Content.ReadAsStringAsync();
            var pathMatch = Regex.Match(uploadResult, @"""Path"":""([^""]+)""");
            Assert.IsTrue(pathMatch.Success, "Failed to extract file path from upload response");
            privateFilePath = pathMatch.Groups[1].Value;
        }

        // Now submit the form with the logical path
        var createPrivateContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Name", "Private Secret Document" },
            { "FilePath", privateFilePath },
            { "Status", "1" }, // Active
            { "IsPublic", "false" },
            { "__RequestVerificationToken", createPrivateContractToken }
        });
        var createPrivateResponse = await _http.PostAsync("/ManageContract/Create", createPrivateContent);
        Assert.AreEqual(HttpStatusCode.Found, createPrivateResponse.StatusCode);

        // 4. Create a normal user
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

        // 5. Log in as the normal user and verify visibility
        await _http.GetAsync("/Account/LogOff");

        loginToken = await GetAntiCsrfToken("/Account/Login");
        loginResponse = await _http.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", email },
            { "Password", password },
            { "__RequestVerificationToken", loginToken }
        }));
        Assert.AreEqual(HttpStatusCode.Found, loginResponse.StatusCode);

        var myContractsResponse = await _http.GetAsync("/Contract/Index");
        myContractsResponse.EnsureSuccessStatusCode();
        var myContractsHtml = await myContractsResponse.Content.ReadAsStringAsync();

        // Should see public contract
        Assert.Contains("Public Company Policy", myContractsHtml);
        // Should NOT see private contract
        Assert.DoesNotContain("Private Secret Document", myContractsHtml);

        // 6. Log in as admin and verify visibility
        await _http.GetAsync("/Account/LogOff");
        loginToken = await GetAntiCsrfToken("/Account/Login");
        await _http.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", "admin" },
            { "Password", "admin123" },
            { "__RequestVerificationToken", loginToken }
        }));

        var adminContractsResponse = await _http.GetAsync("/Contract/Index");
        adminContractsResponse.EnsureSuccessStatusCode();
        var adminContractsHtml = await adminContractsResponse.Content.ReadAsStringAsync();

        // Should see both
        Assert.Contains("Public Company Policy", adminContractsHtml);
        Assert.Contains("Private Secret Document", adminContractsHtml);

        // 7. Verify Manage page
        var manageResponse = await _http.GetAsync("/ManageContract/Index");
        manageResponse.EnsureSuccessStatusCode();
        var manageHtml = await manageResponse.Content.ReadAsStringAsync();
        Assert.Contains("Public Company Policy", manageHtml);
        Assert.Contains("Private Secret Document", manageHtml);
    }
}
