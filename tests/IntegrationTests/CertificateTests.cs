using System.Net;
using Aiursoft.DbTools;
using Aiursoft.EmployeeCenter.Entities;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class CertificateTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public CertificateTests()
    {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            AllowAutoRedirect = false
        };
        _port = Aiursoft.CSTools.Tools.Network.GetAvailablePort();
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

    [TestMethod]
    public async Task GetEmploymentCertificatePageTest()
    {
        var response = await _http.GetAsync("/Certificate/Employment");
        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
        Assert.IsTrue(response.Headers.Location!.ToString().Contains("Account/Login"));
    }
}
