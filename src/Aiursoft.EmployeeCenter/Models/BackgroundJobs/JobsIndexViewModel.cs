using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.BackgroundJobs;

public class JobsIndexViewModel : UiStackLayoutViewModel
{
    [Display(Name = "All Recent Jobs")]
    public IEnumerable<JobInfo> AllRecentJobs { get; init; } = [];
}
