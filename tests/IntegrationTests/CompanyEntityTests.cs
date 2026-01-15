namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class CompanyEntityTests : TestBase
{
    [TestMethod]
    public async Task CreateAndVerifyZipCodeTest()
    {
        // 1. Login as admin
        await LoginAsAdmin();

        // 2. Create a Company Entity with ZipCode
        var createResponse = await PostForm("/CompanyEntity/Create", new Dictionary<string, string>
        {
            { "CompanyName", "Test Company" },
            { "EntityCode", "123456789" },
            { "OfficeAddress", "123 Office St" },
            { "ZipCode", "200000" }
        });
        Assert.AreEqual(HttpStatusCode.Found, createResponse.StatusCode);

        // 3. Verify in Manage page (as admin)
        var manageResponse = await Http.GetAsync("/CompanyEntity/Manage");
        manageResponse.EnsureSuccessStatusCode();
        var manageHtml = await manageResponse.Content.ReadAsStringAsync();
        Assert.Contains("Test Company", manageHtml);

        // 4. Get the ID of the newly created entity
        var match = Regex.Match(manageHtml, @"/CompanyEntity/Details/(\d+)");
        Assert.IsTrue(match.Success, "Could not find Details link for the created company entity");
        var entityId = match.Groups[1].Value;

        // 5. Verify Details page (as admin)
        var detailsResponse = await Http.GetAsync($"/CompanyEntity/Details/{entityId}");
        detailsResponse.EnsureSuccessStatusCode();
        var detailsHtml = await detailsResponse.Content.ReadAsStringAsync();
        Assert.Contains("Test Company", detailsHtml);
        Assert.Contains("200000", detailsHtml);

        // 6. Login as normal user
        await Http.GetAsync("/Account/LogOff");
        await RegisterAndLoginAsync();

        // 7. Verify Details page (as normal user)
        var userDetailsResponse = await Http.GetAsync($"/CompanyEntity/Details/{entityId}");
        userDetailsResponse.EnsureSuccessStatusCode();
        var userDetailsHtml = await userDetailsResponse.Content.ReadAsStringAsync();
        Assert.Contains("Test Company", userDetailsHtml);
        Assert.Contains("200000", userDetailsHtml);
    }
}
