using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LeaveViewModels;

public class TeamCalendarViewModel : UiStackLayoutViewModel
{
    public List<TeamMemberLeave> TeamLeaves { get; set; } = new();
}