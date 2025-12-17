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

        // Get the user ID for the configured username
        var userId = await GetUserIdByUsernameAsync(baseUrl, _gitLabSettings.ProjectMustBeStaredBy);

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
        var projectsDto = JsonConvert.DeserializeObject<List<GitLabProjectDto>>(projectsResponse) ?? new List<GitLabProjectDto>();

        // Filter out archived projects
        var activeProjects = projectsDto.Where(p => !p.Archived).ToList();

        var resultProjects = new List<GitLabProject>();

        // Fetch READMEs in parallel
        var tasks = activeProjects.Select(async projectDto =>
        {
            var project = new GitLabProject
            {
                Id = projectDto.Id,
                Name = projectDto.Name,
                Description = projectDto.Description ?? string.Empty,
                WebUrl = projectDto.WebUrl,
                HttpUrlToRepo = projectDto.HttpUrlToRepo,
                SshUrlToRepo = projectDto.SshUrlToRepo,
                Archived = projectDto.Archived,
                Topics = projectDto.Topics ?? new List<string>()
            };

            try
            {
                var readmeUrl = $"{baseUrl}/api/v4/projects/{projectDto.Id}/repository/files/README.md/raw?ref={projectDto.DefaultBranch}";
                var readmeContent = await _httpClient.GetStringAsync(readmeUrl);
                project.Badges = ExtractBadges(readmeContent);
            }
            catch
            {
                // Ignore if README not found or other errors
            }

            // Check if the project is starred by the required user
            if (userId.HasValue)
            {
                project.IsStarredByRequiredUser = await IsProjectStarredByUserAsync(baseUrl, projectDto.Id, userId.Value);
            }

            lock (resultProjects)
            {
                resultProjects.Add(project);
            }
        });

        await Task.WhenAll(tasks);

        return resultProjects;
    }

    private List<Badge> ExtractBadges(string markdown)
    {
        var badges = new List<Badge>();
        var lines = markdown.Split('\n');
        // Regex for linked image: [![Alt](ImgUrl)](LinkUrl)
        var linkedImageRegex = new System.Text.RegularExpressions.Regex(@"^\[!\[(.*?)\]\((.*?)\)\]\((.*?)\)");
        // Regex for image: ![Alt](ImgUrl)
        var imageRegex = new System.Text.RegularExpressions.Regex(@"^!\[(.*?)\]\((.*?)\)");

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine)) continue;
            if (trimmedLine.StartsWith("#")) continue; // Skip headers

            // Attempt to match linked image first
            var linkedMatch = linkedImageRegex.Match(trimmedLine);
            if (linkedMatch.Success)
            {
                badges.Add(new Badge
                {
                    AltText = linkedMatch.Groups[1].Value,
                    ImageUrl = linkedMatch.Groups[2].Value,
                    LinkUrl = linkedMatch.Groups[3].Value
                });
                continue;
            }

            // Attempt to match simple image
            var imageMatch = imageRegex.Match(trimmedLine);
            if (imageMatch.Success)
            {
                badges.Add(new Badge
                {
                    AltText = imageMatch.Groups[1].Value,
                    ImageUrl = imageMatch.Groups[2].Value
                });
                continue;
            }

            // Detect end of badge section: a line that is not a badge?
            // The prompt says "continuous badge part".
            // If the line has content but is not a badge, we stop assuming the badge section is over.
            // But sometimes badges are inline. The user example shows one per line basically or same line.
            // If the line doesn't start with ! or [, it's likely text.
            if (!trimmedLine.StartsWith("[") && !trimmedLine.StartsWith("!"))
            {
                // If we already found badges, break.
                if (badges.Count > 0)
                {
                    break;
                }
            }
        }
        return badges;
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

    private async Task<int?> GetUserIdByUsernameAsync(string baseUrl, string username)
    {
        try
        {
            var userSearchUrl = $"{baseUrl}/api/v4/users?username={username}";
            var userResponse = await _httpClient.GetStringAsync(userSearchUrl);
            var users = JsonConvert.DeserializeObject<List<GitLabUser>>(userResponse);

            if (users != null && users.Count > 0)
            {
                return users[0].Id;
            }
        }
        catch
        {
            // Ignore if user not found or other errors
        }

        return null;
    }

    private async Task<bool> IsProjectStarredByUserAsync(string baseUrl, int projectId, int userId)
    {
        try
        {
            var starrersUrl = $"{baseUrl}/api/v4/projects/{projectId}/starrers";
            var starrersResponse = await _httpClient.GetStringAsync(starrersUrl);
            var starrers = JsonConvert.DeserializeObject<List<GitLabUser>>(starrersResponse);

            return starrers?.Any(s => s.Id == userId) ?? false;
        }
        catch
        {
            // If we can't fetch starrers, assume not starred
            return false;
        }
    }

    // Internal DTOs for GitLab API responses
    private class GitLabGroup
    {
        public int Id { get; set; }
    }

    private class GitLabUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
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

        [JsonProperty("default_branch")]
        public string DefaultBranch { get; set; } = "master";
    }
}
