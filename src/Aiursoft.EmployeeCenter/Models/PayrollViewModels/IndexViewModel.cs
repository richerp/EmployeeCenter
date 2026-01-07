using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.PayrollViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel()
    {
        PageTitle = "My Payrolls";
    }

    public List<Payroll> Payrolls { get; set; } = [];

    // Chart data for salary trend visualization
    public List<string> ChartLabels { get; set; } = [];
    public List<decimal> ChartTotalAmounts { get; set; } = [];
    public List<decimal> ChartBaseSalaries { get; set; } = [];
}
