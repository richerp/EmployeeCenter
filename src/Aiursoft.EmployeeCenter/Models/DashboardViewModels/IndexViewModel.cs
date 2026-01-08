using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.DashboardViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel()
    {
        PageTitle = "Dashboard";
    }

    public required List<OnboardingTask> Tasks { get; set; }
    public required List<OnboardingTaskLog> Logs { get; set; }
    public required User User { get; set; }
}
