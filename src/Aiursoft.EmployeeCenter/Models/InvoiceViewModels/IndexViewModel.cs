using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.InvoiceViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public List<CompanyEntity> Entities { get; set; } = new();
}
