using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LeaveViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    /// <summary>
    /// Total annual leave allocated for current year
    /// </summary>
    public decimal AnnualLeaveAllocation { get; set; }

    /// <summary>
    /// Annual leave carried over from previous year
    /// </summary>
    public decimal CarriedOverAnnualLeave { get; set; }

    /// <summary>
    /// Remaining annual leave (including carried over)
    /// </summary>
    public decimal RemainingAnnualLeave { get; set; }

    /// <summary>
    /// Total sick leave allocated for current year
    /// </summary>
    public decimal SickLeaveAllocation { get; set; }

    /// <summary>
    /// Remaining sick leave for current year
    /// </summary>
    public decimal RemainingSickLeave { get; set; }

    /// <summary>
    /// All leave applications (pending and historical)
    /// </summary>
    public List<LeaveApplication> LeaveHistory { get; set; } = new();

    /// <summary>
    /// Whether user has remaining leave days to apply
    /// </summary>
    public bool CanApplyLeave { get; set; }

    /// <summary>
    /// Current year
    /// </summary>
    public int CurrentYear { get; set; }

    /// <summary>
    /// Next upcoming approved leave starting within 14 days (if any)
    /// </summary>
    public LeaveApplication? NextUpcomingLeave { get; set; }

    /// <summary>
    /// Whether there is an approved leave starting within 14 days
    /// </summary>
    public bool HasUpcomingLeaveWithin14Days { get; set; }
}
