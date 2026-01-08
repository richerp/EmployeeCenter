using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ManageOnboardingViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel() => PageTitle = "Manage Onboarding Tasks";
    public required List<OnboardingTask> Tasks { get; set; }
    public List<EmployeeProgress>? EmployeeProgresses { get; set; }
}

public class EmployeeProgress
{
    public required User User { get; set; }
    public int CompletedTasksCount { get; set; }
    public int TotalTasksCount { get; set; }
    public double ProgressPercentage => TotalTasksCount == 0 ? 0 : (double)CompletedTasksCount / TotalTasksCount * 100;
}
