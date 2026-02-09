using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LeaveViewModels;

public class TeamCalendarViewModel : UiStackLayoutViewModel
{
    public List<TeamMemberLeave> TeamLeaves { get; set; } = new();

    public bool CanApproveAnyLeave { get; set; }

    public User? SearchedUser { get; set; }

    public List<LeaveApplication> SearchedUserLeaves { get; set; } = new();
}