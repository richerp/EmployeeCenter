using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ContractViewModels;

public class ManageViewModel : UiStackLayoutViewModel
{
    public ManageViewModel()
    {
        PageTitle = "Manage Contracts";
    }

    [Display(Name = "Contracts")]
    public IEnumerable<Contract> Contracts { get; set; } = new List<Contract>();
}
