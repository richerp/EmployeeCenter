using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ReportLineViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel()
    {
        PageTitle = "Report Line";
    }

    public required User TargetUser { get; set; }
    public List<User> ManagerChain { get; set; } = new();
    public List<User> Subordinates { get; set; } = new();
    public List<User> Peers { get; set; } = new();
    public bool IsViewingSelf { get; set; }
    public string? Error { get; set; }
}
