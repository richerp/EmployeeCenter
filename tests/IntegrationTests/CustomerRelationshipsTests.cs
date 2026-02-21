using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class CustomerRelationshipsTests : TestBase
{
    [TestMethod]
    public async Task AdminCanAccessManagePage()
    {
        await LoginAsAdmin();
        var response = await Http.GetAsync("/CustomerRelationships/Manage");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task AdminCanCreateRelationship()
    {
        await LoginAsAdmin();
        
        var name = "Test Customer " + Guid.NewGuid();

        var response = await PostForm("/CustomerRelationships/Create", new Dictionary<string, string>
        {
            { "Name", name },
            { "Email", "test@customer.com" },
            { "Phone", "1234567890" },
            { "Address", "Test Address" },
            { "Remark", "Test Remark" }
        });

        AssertRedirect(response, "/CustomerRelationships/Manage");

        // Verify it's created
        var managePage = await Http.GetStringAsync("/CustomerRelationships/Manage");
        Assert.IsTrue(managePage.Contains(name));
    }

    [TestMethod]
    public async Task UserCanAccessIndexButNotManage()
    {
        await RegisterAndLoginAsync();

        // Can access Index
        var indexResponse = await Http.GetAsync("/CustomerRelationships/Index");
        Assert.AreEqual(HttpStatusCode.OK, indexResponse.StatusCode);

        // Cannot access Manage (should redirect to Code403)
        var manageResponse = await Http.GetAsync("/CustomerRelationships/Manage");
        Assert.AreEqual(HttpStatusCode.Found, manageResponse.StatusCode);
        var location = manageResponse.Headers.Location?.ToString();
        Assert.IsTrue(location?.Contains("Code403") == true, $"Expected Code403, but got {location}");

        // Cannot access Create
        var createResponse = await Http.GetAsync("/CustomerRelationships/Create");
        Assert.AreEqual(HttpStatusCode.Found, createResponse.StatusCode);
        location = createResponse.Headers.Location?.ToString();
        Assert.IsTrue(location?.Contains("Code403") == true, $"Expected Code403, but got {location}");
    }
}
