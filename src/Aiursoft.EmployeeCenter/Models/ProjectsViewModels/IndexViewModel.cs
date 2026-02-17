using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ProjectsViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel()
    {
        PageTitle = "Projects";
    }

    [Display(Name = "Projects by Tags")]
    public required List<ProjectsByTag> ProjectsByTags { get; set; }

    [Display(Name = "Required Star Username")]
    public required string RequiredStarUsername { get; set; }

    [Display(Name = "Required GitHub Org Mirrored")]
    public required string RequiredGitHubOrgMirrored { get; set; }
}
