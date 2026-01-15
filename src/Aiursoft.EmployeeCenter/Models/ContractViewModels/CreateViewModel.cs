using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ContractViewModels;

public class CreateViewModel : UiStackLayoutViewModel
{
    public CreateViewModel()
    {
        PageTitle = "Create New Contract";
    }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [MinLength(2)]
    [Display(Name = "Contract File")]
    [RegularExpression(@"^contract.*", ErrorMessage = "Please upload a valid contract file.")]
    public string? FilePath { get; set; }

    [Display(Name = "Is Public")]
    public bool IsPublic { get; set; }

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.PendingSignature;
}
