using Aiursoft.EmployeeCenter.Entities;

using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LeaveViewModels;

public class HistoryViewModel : UiStackLayoutViewModel
{
    public required List<LeaveApplication> ApprovedLeaves { get; set; }
}
