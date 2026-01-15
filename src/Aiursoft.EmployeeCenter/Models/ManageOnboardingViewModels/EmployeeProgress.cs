using Aiursoft.EmployeeCenter.Entities;

namespace Aiursoft.EmployeeCenter.Models.ManageOnboardingViewModels;

public class EmployeeProgress
{
    public required User User { get; set; }
    public int CompletedTasksCount { get; set; }
    public int TotalTasksCount { get; set; }
    public double ProgressPercentage => TotalTasksCount == 0 ? 0 : (double)CompletedTasksCount / TotalTasksCount * 100;
}