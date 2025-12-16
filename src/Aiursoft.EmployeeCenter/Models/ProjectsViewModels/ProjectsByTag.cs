namespace Aiursoft.EmployeeCenter.Models.ProjectsViewModels;

public class ProjectsByTag
{
    public string Tag { get; set; } = string.Empty;
    public List<GitLabProject> Projects { get; set; } = new();
}
