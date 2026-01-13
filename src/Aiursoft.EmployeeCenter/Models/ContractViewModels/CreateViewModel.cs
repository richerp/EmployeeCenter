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

    [Display(Name = "User (Optional)")]
    public string? UserId { get; set; }

    public IEnumerable<User> AllUsers { get; set; } = new List<User>();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Contract File")]
    public IFormFile? File { get; set; }

    [Display(Name = "Is Public")]
    public bool IsPublic { get; set; }

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.PendingSignature;
}
