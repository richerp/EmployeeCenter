using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ServersViewModels;

public class IndexServerViewModel : UiStackLayoutViewModel
{
    public IEnumerable<Server> Servers { get; set; } = new List<Server>();
}
