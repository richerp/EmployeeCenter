using System.Net;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class CompanyEntityTests : TestBase
{
    [TestMethod]
    public async Task CreateCompanyEntity_WithInvalidFiles_Fails()
    {
        await LoginAsAdmin();

        var formData = new Dictionary<string, string>
        {
            { "CompanyName", "Test Company" },
            { "EntityCode", "123456" },
            { "LicensePath", "company-certs/fake-license.pdf" }, // Not existing
            { "OrganizationCertificatePath", "company-certs/fake-cert.pdf" } // Not existing
        };

        var response = await PostForm("/CompanyEntity/Create", formData);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode); // Should return View (error), not Redirect
        var html = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(html, "File not found or lost");
    }

    [TestMethod]
    public async Task CreateCompanyEntity_WithNoFiles_Succeeds()
    {
        await LoginAsAdmin();

        var formData = new Dictionary<string, string>
        {
            { "CompanyName", "Test Company 2" },
            { "EntityCode", "654321" }
        };

        var response = await PostForm("/CompanyEntity/Create", formData);

        AssertRedirect(response, "/CompanyEntity/Manage");

        // Verify in DB
        using var scope = Server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
        var entity = await db.CompanyEntities.FirstOrDefaultAsync(e => e.EntityCode == "654321");
        Assert.IsNotNull(entity);
        Assert.AreEqual("Test Company 2", entity.CompanyName);
    }
}
