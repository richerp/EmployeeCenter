using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.AssetsViewModels;

public class ManageModelsViewModel : UiStackLayoutViewModel
{
    public List<AssetModel> Models { get; set; } = new();
    public List<AssetCategory> AllCategories { get; set; } = new();

    [Required]
    public int NewCategoryId { get; set; }

    [Required]
    [MaxLength(50)]
    public string NewBrand { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NewModelName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? NewSpecs { get; set; }
}
