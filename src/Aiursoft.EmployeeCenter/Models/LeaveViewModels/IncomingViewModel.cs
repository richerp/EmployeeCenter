using Aiursoft.EmployeeCenter.Entities;

using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LeaveViewModels;

public class IncomingViewModel : UiStackLayoutViewModel
{
    public required List<LeaveApplication> IncomingLeaves { get; set; }
}
