using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ServicesViewModels;

public class ManageProvidersViewModel : UiStackLayoutViewModel
{
    public IEnumerable<Provider> Providers { get; set; } = new List<Provider>();

    [Required]
    [MaxLength(100)]
    [Display(Name = "New Provider Name")]
    public string NewName { get; set; } = string.Empty;
}
