using Aiursoft.EmployeeCenter.Configuration;
using Aiursoft.EmployeeCenter.Models.ProjectsViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.EmployeeCenter.Services.GitLab;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Localization;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize]
[LimitPerMin]
public class ProjectsController : Controller
{
    private readonly GitLabService _gitLabService;
    private readonly GlobalSettingsService _globalSettings;
    private readonly IStringLocalizer<ProjectsController> _localizer;

    public ProjectsController(
        GitLabService gitLabService,
        GlobalSettingsService globalSettings,
        IStringLocalizer<ProjectsController> localizer)
    {
        _gitLabService = gitLabService;
        _globalSettings = globalSettings;
        _localizer = localizer;
    }

    [RenderInNavBar(
        NavGroupName = "Career",
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
            RequiredStarUsername = await _globalSettings.GetSettingValueAsync(SettingsMap.GitLabProjectMustBeStaredBy),
            RequiredGitHubOrgMirrored = await _globalSettings.GetSettingValueAsync(SettingsMap.GitLabEnsureGitHubOrgMirrored),
            PageTitle = _localizer["Projects"]
        };

        return this.StackView(model);
    }
}
