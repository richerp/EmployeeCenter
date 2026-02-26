namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class ServersTests : TestBase
{
    [TestMethod]
    public async Task TestServersIndex()
    {
        await LoginAsAdmin();
        var response = await Http.GetAsync("/Servers/Index");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(content, "Servers");
    }

    [TestMethod]
    public async Task TestServerCrud()
    {
        await LoginAsAdmin();

        // Create a company entity first
        var db = GetService<EmployeeCenterDbContext>();
        var company = new CompanyEntity
        {
            CompanyName = "Test Company",
            EntityCode = "TC123",
            BaseCurrency = "CNY"
        };
        db.CompanyEntities.Add(company);
        await db.SaveChangesAsync();

        // Create
        var response = await Http.GetAsync("/Servers/Create");
        response.EnsureSuccessStatusCode();

        var postResponse = await PostForm("/Servers/Create", new Dictionary<string, string>
        {
            { "Hostname", "test-server-01" },
            { "ServerIp", "192.168.1.100" },
            { "DetailLink", "https://example.com" },
            { "CompanyEntityId", company.Id.ToString() }
        });

        Assert.AreEqual(HttpStatusCode.Redirect, postResponse.StatusCode);

        db = GetService<EmployeeCenterDbContext>();
        db.ChangeTracker.Clear();
        var server = await db.Servers.FirstOrDefaultAsync(s => s.Hostname == "test-server-01");
        Assert.IsNotNull(server);
        Assert.AreEqual("192.168.1.100", server.ServerIp);
        Assert.AreEqual(company.Id, server.CompanyEntityId);

        // Edit
        var editResponse = await PostForm("/Servers/Edit", new Dictionary<string, string>
        {
            { "Id", server.Id.ToString() },
            { "Hostname", "test-server-01-updated" },
            { "ServerIp", "192.168.1.101" },
            { "CompanyEntityId", company.Id.ToString() }
        });

        Assert.AreEqual(HttpStatusCode.Redirect, editResponse.StatusCode);

        db = GetService<EmployeeCenterDbContext>();
        db.Entry(server).Reload();
        Assert.AreEqual("test-server-01-updated", server.Hostname);
        Assert.AreEqual(company.Id, server.CompanyEntityId);

        // Delete
        var deleteResponse = await PostForm($"/Servers/Delete/{server.Id}", new Dictionary<string, string>());
        Assert.AreEqual(HttpStatusCode.Redirect, deleteResponse.StatusCode);

        db = GetService<EmployeeCenterDbContext>();
        db.ChangeTracker.Clear();
        var deletedServer = await db.Servers.FindAsync(server.Id);
        Assert.IsNull(deletedServer);
    }
}
