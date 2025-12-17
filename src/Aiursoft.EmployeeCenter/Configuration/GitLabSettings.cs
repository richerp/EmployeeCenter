namespace Aiursoft.EmployeeCenter.Configuration;

public class GitLabSettings
{
    public required string OrganizationUrl { get; init; }
    public required string ProjectMustBeStaredBy { get; init; }
}
