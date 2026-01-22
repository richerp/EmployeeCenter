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
            { "IsOnline", "true" },
            { "IsSelfDeveloped", "false" }
        });
        
        Assert.AreEqual(HttpStatusCode.Redirect, postResponse.StatusCode);

        var db = GetService<EmployeeCenterDbContext>();
        var service = await db.Services.FirstOrDefaultAsync(s => s.Domain == "test-service.com");
        Assert.IsNotNull(service);
        Assert.IsTrue(service.IsOnline);
    }
}