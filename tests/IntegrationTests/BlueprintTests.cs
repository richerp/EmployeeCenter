namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class BlueprintTests : TestBase
{
    [TestMethod]
    public async Task GetCreateBlueprintPageAdminAccessTest()
    {
        await LoginAsAdmin();
        var response = await Http.GetAsync("/Blueprint/Create");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();

        // Assert that the old highlight.js scripts are gone
        Assert.DoesNotContain("node_modules/highlight.js/lib/core.js", html);
        Assert.DoesNotContain("node_modules/highlight.js/lib/languages/csharp.js", html);

        // Assert that the new highlight.js cdn-assets script is present
        Assert.Contains("node_modules/@highlightjs/cdn-assets/highlight.min.js", html);

        // Assert that the styles are also updated
        Assert.Contains("node_modules/@highlightjs/cdn-assets/styles/github.min.css", html);
    }
}
