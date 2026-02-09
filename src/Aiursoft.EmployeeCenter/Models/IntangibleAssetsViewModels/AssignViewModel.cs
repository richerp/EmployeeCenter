using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.IntangibleAssetsViewModels;

public class AssignViewModel : UiStackLayoutViewModel
{
    [Required]
    public Guid AssetId { get; set; }

    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Assign to")]
    public string AssigneeId { get; set; } = string.Empty;

    public IEnumerable<User> AllUsers { get; set; } = new List<User>();
}
