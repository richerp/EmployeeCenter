using System.Net;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class AiAssistantTests : TestBase
{
    [TestMethod]
    public async Task GetIndex_Anonymous_RedirectsToLogin()
    {
        // Act
        var response = await Http.GetAsync("/AiAssistant/Index");

        // Assert
        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
        Assert.IsTrue(response.Headers.Location?.OriginalString.Contains("/Account/Login") ?? false);
    }

    [TestMethod]
    public async Task GetIndex_Authenticated_ReturnsSuccess()
    {
        // Arrange
        await LoginAsAdmin();

        // Act
        var response = await Http.GetAsync("/AiAssistant/Index");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("AI Assistant"));
        Assert.IsTrue(content.Contains("Chat with EmployeeCenter AI"));
    }

    [TestMethod]
    public async Task Ask_Authenticated_ReturnsErrorWhenNoAgent()
    {
        // Arrange
        await LoginAsAdmin();
        var request = new { Question = "Hello" };

        // Act
        var response = await Http.PostAsJsonAsync("/AiAssistant/Ask", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("Agent failed to respond"));
    }
}
