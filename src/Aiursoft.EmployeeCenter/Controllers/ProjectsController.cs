using Aiursoft.EmployeeCenter.Configuration;
using Aiursoft.EmployeeCenter.Models.ProjectsViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.EmployeeCenter.Services.GitLab;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aiursoft.EmployeeCenter.Controllers;

[LimitPerMin]
public class ProjectsController : Controller
{
    private readonly GitLabService _gitLabService;
    private readonly GitLabSettings _gitLabSettings;

    public ProjectsController(GitLabService gitLabService, IOptions<GitLabSettings> gitLabSettings)
    {
        _gitLabService = gitLabService;
        _gitLabSettings = gitLabSettings.Value;
    }

    [RenderInNavBar(
        NavGroupName = "Features",
        NavGroupOrder = 1,
        CascadedLinksGroupName = "Development",
        CascadedLinksIcon = "git-branch",
        CascadedLinksOrder = 2,
        LinkText = "Projects",
        LinkOrder = 1)]
    public async Task<IActionResult> Index()
    {
        var projects = await _gitLabService.GetAllProjectsAsync();
        var projectsByTags = _gitLabService.GroupProjectsByFirstTag(projects);

        var model = new IndexViewModel
        {
            ProjectsByTags = projectsByTags,
            RequiredStarUsername = _gitLabSettings.ProjectMustBeStaredBy
        };

        return this.StackView(model);
    }
}
