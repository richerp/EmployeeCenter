using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LeaveViewModels;

public class TeamCalendarViewModel : UiStackLayoutViewModel
{
    public List<TeamMemberLeave> TeamLeaves { get; set; } = new();
}

public class TeamMemberLeave
{
    public required User User { get; set; }
    public required List<LeaveApplication> Leaves { get; set; }
    public required string Relation { get; set; } // Boss, Direct Report, Colleague
}
