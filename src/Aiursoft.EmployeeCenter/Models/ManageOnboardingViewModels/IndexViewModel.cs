using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ManageOnboardingViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel() => PageTitle = "Manage Onboarding Tasks";
    public required List<OnboardingTask> Tasks { get; set; }
}
