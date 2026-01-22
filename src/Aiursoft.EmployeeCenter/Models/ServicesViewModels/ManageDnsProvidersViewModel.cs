using Aiursoft.EmployeeCenter.Entities;
using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ServicesViewModels;

public class ManageDnsProvidersViewModel : UiStackLayoutViewModel
{
    public List<DnsProvider> DnsProviders { get; set; } = new();

    [Required]
    [MaxLength(100)]
    [Display(Name = "DNS Provider Name")]
    public string? NewName { get; set; }

    [MaxLength(200)]
    [Display(Name = "Description")]
    public string? NewDescription { get; set; }
}
