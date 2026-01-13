using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.PayrollViewModels;

public class ManageViewModel : UiStackLayoutViewModel
{
    public ManageViewModel()
    {
        PageTitle = "Manage Payrolls";
    }

    public List<Payroll> Payrolls { get; set; } = [];
    public List<User> AllUsers { get; set; } = [];
}
