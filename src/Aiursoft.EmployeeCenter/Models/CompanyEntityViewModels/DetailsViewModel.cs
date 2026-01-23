using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.CompanyEntityViewModels;

public class DetailsViewModel : UiStackLayoutViewModel
{
    public DetailsViewModel()
    {
        PageTitle = "Company Entity Details";
    }
    public required CompanyEntity Entity { get; set; }
    public IEnumerable<Server> Servers { get; set; } = new List<Server>();
}
