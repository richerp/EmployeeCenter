using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ServicesViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public List<Service> Services { get; set; } = new();
}
