using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ProjectsViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel()
    {
        PageTitle = "Projects";
    }

    public required List<ProjectsByTag> ProjectsByTags { get; set; }
    public required string RequiredStarUsername { get; set; }
}
