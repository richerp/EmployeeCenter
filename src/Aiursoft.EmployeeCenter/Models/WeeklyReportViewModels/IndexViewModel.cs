using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.WeeklyReportViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel()
    {
        PageTitle = "Weekly Report";
    }

    public List<WeeklyReport> Reports { get; set; } = new();
    
    public List<User> AllUsers { get; set; } = new();
    
    public bool CanCreateForAnyone { get; set; }
    public bool CanCreate { get; set; }
    
    public bool CurrentWeekSubmitted { get; set; }
    public string? NotepadContent { get; set; }
    
    public string? FilterUserId { get; set; }
    public User? FilterUser { get; set; }
}
