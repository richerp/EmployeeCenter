using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.AssetsViewModels;

public class AssignAssetViewModel : UiStackLayoutViewModel
{
    [Required]
    public Guid AssetId { get; set; }

    public string AssetTag { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Assign To")]
    public string AssigneeId { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public List<User> AllUsers { get; set; } = new();
}
