using System.Net;
using Aiursoft.EmployeeCenter.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class RequirementTests : TestBase
{
    [TestMethod]
    public async Task GetApprovedProjectsPageTest()
    {
        await LoginAsAdmin();
        var response = await Http.GetAsync("/Requirements/Index");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMyRequirementsPageTest()
    {
        await LoginAsAdmin();
        var response = await Http.GetAsync("/Requirements/My");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetHistoryPageTest()
    {
        await LoginAsAdmin();
        var response = await Http.GetAsync("/Requirements/History");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetManageRequirementsPageAdminAccessTest()
    {
        await LoginAsAdmin();
        var response = await Http.GetAsync("/Requirements/Manage");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetManageRequirementsPageUserAccessTest()
    {
        await RegisterAndLoginAsync();
        var response = await Http.GetAsync("/Requirements/Manage");
        AssertRedirect(response, "/Error/Code403", exact: false);
    }

    [TestMethod]
    public async Task CreateRequirementTest()
    {
        await RegisterAndLoginAsync();
        var response = await Http.GetAsync("/Requirements/Create");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var postResponse = await PostForm("/Requirements/Create", new Dictionary<string, string>
        {
            { "Title", "Test Requirement" },
            { "InputMarkdown", "# Test\nContent" }
        });

        // Should redirect to View
        Assert.AreEqual(HttpStatusCode.Found, postResponse.StatusCode);
        
        using var scope = Server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
        var requirement = await db.Requirements.FirstOrDefaultAsync(r => r.Title == "Test Requirement");
        Assert.IsNotNull(requirement);
        Assert.AreEqual(RequirementStatus.PendingApproval, requirement.Status);
    }

    [TestMethod]
    public async Task CommentRequirementTest()
    {
        await LoginAsAdmin();
        // Create a requirement first
        await PostForm("/Requirements/Create", new Dictionary<string, string>
        {
            { "Title", "Comment Test" },
            { "InputMarkdown", "Content" }
        });

        int requirementId;
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            requirementId = (await db.Requirements.FirstAsync(r => r.Title == "Comment Test")).Id;
        }

        var commentResponse = await PostForm($"/Requirements/Comment/{requirementId}", new Dictionary<string, string>
        {
            { "content", "Test Comment" }
        }, tokenUrl: $"/Requirements/View/{requirementId}");

        Assert.AreEqual(HttpStatusCode.Found, commentResponse.StatusCode);

        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var comment = await db.RequirementComments.FirstOrDefaultAsync(c => c.RequirementId == requirementId && c.Content == "Test Comment");
            Assert.IsNotNull(comment);
        }
    }

    [TestMethod]
    public async Task ApproveRequirementTest()
    {
        await LoginAsAdmin();
        // Create a requirement
        await PostForm("/Requirements/Create", new Dictionary<string, string>
        {
            { "Title", "Approve Test" },
            { "InputMarkdown", "Content" }
        });

        int requirementId;
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            requirementId = (await db.Requirements.FirstAsync(r => r.Title == "Approve Test")).Id;
        }

        var approveResponse = await PostForm($"/Requirements/Approve/{requirementId}", new Dictionary<string, string>(), tokenUrl: $"/Requirements/View/{requirementId}");

        Assert.AreEqual(HttpStatusCode.Found, approveResponse.StatusCode);

        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var requirement = await db.Requirements.FindAsync(requirementId);
            Assert.AreEqual(RequirementStatus.Approved, requirement?.Status);
        }
    }
}
