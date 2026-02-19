using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ContractViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel()
    {
        PageTitle = "Contracts";
    }

    [Display(Name = "Contracts")]
    public IEnumerable<Contract> Contracts { get; set; } = new List<Contract>();
}
