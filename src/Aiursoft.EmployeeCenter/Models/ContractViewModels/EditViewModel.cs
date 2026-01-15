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
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(150)]
    [Display(Name = "New Contract File (Optional)")]
    [RegularExpression(@"^contract.*", ErrorMessage = "Please upload a valid contract file.")]
    public string? FilePath { get; set; }

    [Display(Name = "Is Public")]
    public bool IsPublic { get; set; }

    [Required]
    public ContractStatus Status { get; set; }
}
