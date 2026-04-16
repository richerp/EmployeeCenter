using Aiursoft.EmployeeCenter.Configuration;
using Aiursoft.EmployeeCenter.Models.AiAssistantViewModels;
using Aiursoft.UiStack.Navigation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

using System.Text.Json.Serialization;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize]
public class AiAssistantController(
    IOptions<AppSettings> appSettings,
    IHttpClientFactory httpClientFactory) : Controller
{
    [RenderInNavBar(
        NavGroupName = "Personal",
        NavGroupOrder = 10,
        CascadedLinksGroupName = "AI Assistant",
        CascadedLinksIcon = "sparkles",
        CascadedLinksOrder = 100,
        LinkText = "Chat",
        LinkOrder = 1)]
    public IActionResult Index()
    {
        return View(new IndexViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] AskRequest request)
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync(appSettings.Value.Agent.Endpoint, new
        {
            system_prompt = "You are an AI assistant for EmployeeCenter. Answer questions based on the provided context. If you don't know, say you don't know.",
            question = request.Question
        });

        if (!response.IsSuccessStatusCode)
        {
            return BadRequest(new { error = "Agent is not responding." });
        }

        var result = await response.Content.ReadFromJsonAsync<AgentResponse>();
        return Json(new { answer = result?.Answer ?? "No answer received." });
    }
}

public class AskRequest
{
    public required string Question { get; set; }
}

public class AgentResponse
{
    [JsonPropertyName("answer")]
    public string? Answer { get; set; }
}
