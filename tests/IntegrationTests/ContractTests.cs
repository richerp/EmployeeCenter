using System.Net;
using System.Text.RegularExpressions;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.EmployeeCenter.Entities;
using Microsoft.EntityFrameworkCore;
using static Aiursoft.WebTools.Extends;

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
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            Assert.IsNotNull(user);
            userId = user.Id;
        }

        // 4. Log in as admin again to create contract
        await _http.GetAsync("/Account/LogOff");
        loginToken = await GetAntiCsrfToken("/Account/Login");
        await _http.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", "admin" },
            { "Password", "admin123" },
            { "__RequestVerificationToken", loginToken }
        }));

        var createContractToken = await GetAntiCsrfToken("/ManageContract/Create");
        
        using (var content = new MultipartFormDataContent())
        {
            content.Add(new StringContent(userId), "UserId");
            content.Add(new StringContent("Test Contract"), "Name");
            content.Add(new StringContent("1"), "Status"); // Active
            content.Add(new StringContent(createContractToken), "__RequestVerificationToken");
            
            var fileContent = new ByteArrayContent([ 1, 2, 3 ]);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "File", "test.pdf");

            var createContractResponse = await _http.PostAsync("/ManageContract/Create", content);
            Assert.AreEqual(HttpStatusCode.Found, createContractResponse.StatusCode);
        }

        // 5. Verify contract in Manage page
        var manageResponse = await _http.GetAsync("/ManageContract/Index");
        manageResponse.EnsureSuccessStatusCode();
        var manageHtml = await manageResponse.Content.ReadAsStringAsync();
        Assert.Contains("Test Contract", manageHtml);
        Assert.Contains(userName, manageHtml);

        // 6. Log in as the normal user and verify they can see their contract
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
        Assert.Contains("Test Contract", myContractsHtml);

        // 7. Log in as admin and create a PUBLIC contract without user
        await _http.GetAsync("/Account/LogOff");
        loginToken = await GetAntiCsrfToken("/Account/Login");
        await _http.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", "admin" },
            { "Password", "admin123" },
            { "__RequestVerificationToken", loginToken }
        }));

        var createPublicContractToken = await GetAntiCsrfToken("/ManageContract/Create");
        using (var content = new MultipartFormDataContent())
        {
            content.Add(new StringContent("Public Company Policy"), "Name");
            content.Add(new StringContent("1"), "Status"); // Active
            content.Add(new StringContent("true"), "IsPublic");
            content.Add(new StringContent(createPublicContractToken), "__RequestVerificationToken");
            
            var fileContent = new ByteArrayContent([ 1, 2, 3 ]);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "File", "policy.pdf");

            var createContractResponse = await _http.PostAsync("/ManageContract/Create", content);
            Assert.AreEqual(HttpStatusCode.Found, createContractResponse.StatusCode);
        }

        // 8. Log in as the normal user and verify they can see the public contract
        await _http.GetAsync("/Account/LogOff");
        loginToken = await GetAntiCsrfToken("/Account/Login");
        await _http.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", email },
            { "Password", password },
            { "__RequestVerificationToken", loginToken }
        }));

        var publicCheckResponse = await _http.GetAsync("/Contract/Index");
        publicCheckResponse.EnsureSuccessStatusCode();
        var publicCheckHtml = await publicCheckResponse.Content.ReadAsStringAsync();
        Assert.Contains("Public Company Policy", publicCheckHtml);
        Assert.Contains("Shared", publicCheckHtml);

        // 9. Create a PRIVATE contract for ANOTHER user as admin
        await _http.GetAsync("/Account/LogOff");
        loginToken = await GetAntiCsrfToken("/Account/Login");
        await _http.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", "admin" },
            { "Password", "admin123" },
            { "__RequestVerificationToken", loginToken }
        }));

        var createPrivateContractToken = await GetAntiCsrfToken("/ManageContract/Create");
        using (var content = new MultipartFormDataContent())
        {
            content.Add(new StringContent("Private Admin Contract"), "Name");
            content.Add(new StringContent("1"), "Status"); // Active
            content.Add(new StringContent("false"), "IsPublic");
            content.Add(new StringContent(createPrivateContractToken), "__RequestVerificationToken");
            
            var fileContent = new ByteArrayContent([ 1, 2, 3 ]);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "File", "private.pdf");

            var createContractResponse = await _http.PostAsync("/ManageContract/Create", content);
            Assert.AreEqual(HttpStatusCode.Found, createContractResponse.StatusCode);
        }

        // 10. Verify normal user CANNOT see the private admin contract
        await _http.GetAsync("/Account/LogOff");
        loginToken = await GetAntiCsrfToken("/Account/Login");
        await _http.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", email },
            { "Password", password },
            { "__RequestVerificationToken", loginToken }
        }));

        var privateCheckResponse = await _http.GetAsync("/Contract/Index");
        privateCheckResponse.EnsureSuccessStatusCode();
        var privateCheckHtml = await privateCheckResponse.Content.ReadAsStringAsync();
        Assert.IsFalse(privateCheckHtml.Contains("Private Admin Contract"));
    }
}
