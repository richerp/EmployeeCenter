namespace Aiursoft.EmployeeCenter.Configuration;

public class GitLabSettings
{
    public required string OrganizationUrl { get; init; }
    public required string ProjectMustBeStaredBy { get; init; }
    public required string EnsureGitHubOrgMirrored { get; init; }
    public string? GitHubToken { get; init; }  // Optional: increases rate limit from 60/hr to 5000/hr
}
