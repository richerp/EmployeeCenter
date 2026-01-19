using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.AssetsViewModels;

public class ManageVendorsViewModel : UiStackLayoutViewModel
{
    public List<Vendor> Vendors { get; set; } = new();

    [Required]
    [MaxLength(100)]
    public string NewName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NewContactInfo { get; set; }
}
