using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.AssetsViewModels;

public class ManageCategoriesViewModel : UiStackLayoutViewModel
{
    public List<AssetCategory> Categories { get; set; } = new();

    [Required]
    [MaxLength(50)]
    public string NewName { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string NewCode { get; set; } = string.Empty;
}
