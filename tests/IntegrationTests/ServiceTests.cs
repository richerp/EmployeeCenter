namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class ServiceTests : TestBase
{
    [TestMethod]
    public async Task TestServiceIndex()
    {
        await LoginAsAdmin();
        var response = await Http.GetAsync("/Services/Index");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Services", content);
    }

    [TestMethod]
    public async Task TestServiceCreate()
    {
        await LoginAsAdmin();

        var response = await Http.GetAsync("/Services/Create");
        response.EnsureSuccessStatusCode();

        var postResponse = await PostForm("/Services/Create", new Dictionary<string, string>
        {
            { "Domain", "test-service.com" },
            { "Status", "1" },
            { "Purpose", "1" },
            { "IsSelfDeveloped", "false" }
        });
        
        Assert.AreEqual(HttpStatusCode.Redirect, postResponse.StatusCode);

        var db = GetService<EmployeeCenterDbContext>();
        var service = await db.Services.FirstOrDefaultAsync(s => s.Domain == "test-service.com");
        Assert.IsNotNull(service);
        Assert.AreEqual(ServiceStatus.Running, service.Status);
    }

    [TestMethod]
    public async Task TestProviders()
    {
        await LoginAsAdmin();
        
        // Create Provider
        var response = await PostForm("/Services/CreateProvider", new Dictionary<string, string>
        {
            { "NewName", "TestProvider" }
        });
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);

        var db = GetService<EmployeeCenterDbContext>();
        var provider = await db.Providers.FirstOrDefaultAsync(p => p.Name == "TestProvider");
        Assert.IsNotNull(provider);

        // Delete Provider
        var deleteResponse = await PostForm($"/Services/DeleteProvider/{provider.Id}", new Dictionary<string, string>());
        Assert.AreEqual(HttpStatusCode.Redirect, deleteResponse.StatusCode);

        db = GetService<EmployeeCenterDbContext>();
        db.ChangeTracker.Clear();
        var deletedProvider = await db.Providers.FindAsync(provider.Id);
        Assert.IsNull(deletedProvider);
    }
}