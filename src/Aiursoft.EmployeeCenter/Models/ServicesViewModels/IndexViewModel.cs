using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ServicesViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    [Display(Name = "Services")]
    public List<Service> Services { get; set; } = new();
}
