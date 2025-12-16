using Aiursoft.EmployeeCenter.Configuration;
using Aiursoft.EmployeeCenter.Models.ProjectsViewModels;
using Aiursoft.Scanner.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Aiursoft.EmployeeCenter.Services.GitLab;

public class GitLabService : IScopedDependency
{
    private readonly HttpClient _httpClient;
    private readonly GitLabSettings _gitLabSettings;

    public GitLabService(HttpClient httpClient, IOptions<GitLabSettings> gitLabSettings)
    {
        _httpClient = httpClient;
        _gitLabSettings = gitLabSettings.Value;
    }

    public async Task<List<GitLabProject>> GetAllProjectsAsync()
    {
        // Parse organization URL to get the GitLab base URL and organization name
        var orgUrl = _gitLabSettings.OrganizationUrl.TrimEnd('/');
        var segments = orgUrl.Split('/');
        var orgName = segments[^1];
        var baseUrl = string.Join('/', segments[0..^1]);
        
        // Get group ID by name
        var groupSearchUrl = $"{baseUrl}/api/v4/groups?search={orgName}";
        var groupResponse = await _httpClient.GetStringAsync(groupSearchUrl);
        var groups = JsonConvert.DeserializeObject<List<GitLabGroup>>(groupResponse);
        
        if (groups == null || groups.Count == 0)
        {
            return new List<GitLabProject>();
        }
        
        var groupId = groups[0].Id;
        
        // Get all projects in the group
        var projectsUrl = $"{baseUrl}/api/v4/groups/{groupId}/projects?simple=true&per_page=999&visibility=public&page=1";
        var projectsResponse = await _httpClient.GetStringAsync(projectsUrl);
        var projects = JsonConvert.DeserializeObject<List<GitLabProjectDto>>(projectsResponse) ?? new List<GitLabProjectDto>();
        
        // Filter out archived projects and map to our model
        return projects
            .Where(p => !p.Archived)
            .Select(p => new GitLabProject
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description ?? string.Empty,
                WebUrl = p.WebUrl,
                HttpUrlToRepo = p.HttpUrlToRepo,
                SshUrlToRepo = p.SshUrlToRepo,
                Archived = p.Archived,
                Topics = p.Topics ?? new List<string>()
            })
            .ToList();
    }

    public List<ProjectsByTag> GroupProjectsByFirstTag(List<GitLabProject> projects)
    {
        var grouped = projects
            .GroupBy(p => p.Topics.FirstOrDefault() ?? "no-topic")
            .Select(g => new ProjectsByTag
            {
                Tag = g.Key,
                Projects = g.ToList()
            })
            .OrderBy(g => g.Tag)
            .ToList();

        return grouped;
    }

    // Internal DTOs for GitLab API responses
    private class GitLabGroup
    {
        public int Id { get; set; }
    }

    private class GitLabProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        [JsonProperty("web_url")]
        public string WebUrl { get; set; } = string.Empty;
        
        [JsonProperty("http_url_to_repo")]
        public string HttpUrlToRepo { get; set; } = string.Empty;
        
        [JsonProperty("ssh_url_to_repo")]
        public string SshUrlToRepo { get; set; } = string.Empty;
        
        public bool Archived { get; set; }
        public List<string>? Topics { get; set; }
    }
}
