namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class ProjectsControllerTests : TestBase
{
    [TestMethod]
    public async Task GetIndex()
    {
        await LoginAsAdmin();
        var url = "/Projects/Index";

        var response = await Http.GetAsync(url);
        var html = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(html, "GitLab Projects");
        // Since we are hitting the real GitLab API in tests (based on logs), 
        // there should be some projects if the network is available.
        // If it's a mock environment, we might need to adjust this.
    }
}
