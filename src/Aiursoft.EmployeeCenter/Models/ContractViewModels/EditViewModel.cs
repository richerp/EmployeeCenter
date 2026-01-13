using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ContractViewModels;

public class EditViewModel : UiStackLayoutViewModel
{
    public EditViewModel()
    {
        PageTitle = "Edit Contract";
    }

    public int Id { get; set; }

    [Required]
    [Display(Name = "User")]
    public string UserId { get; set; } = string.Empty;

    public string? UserName { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "New Contract File (Optional)")]
    public IFormFile? File { get; set; }

    [Required]
    public ContractStatus Status { get; set; }
}
