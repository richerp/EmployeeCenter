using Aiursoft.EmployeeCenter.Entities;

using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LeaveViewModels;

public class IncomingViewModel : UiStackLayoutViewModel
{
    public required List<LeaveApplication> IncomingLeaves { get; set; }

    public required List<LeaveApplication> ApprovalHistory { get; set; }

    public int PendingCount { get; set; }

    public int ApprovedThisMonth { get; set; }

    public int RejectedThisMonth { get; set; }

    public int TeamOnLeaveCount { get; set; }
}
