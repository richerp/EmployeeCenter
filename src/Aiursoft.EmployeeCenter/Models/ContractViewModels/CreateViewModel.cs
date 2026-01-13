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
    [Display(Name = "User")]
    public string UserId { get; set; } = string.Empty;

    public IEnumerable<User> AllUsers { get; set; } = new List<User>();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Contract File")]
    public IFormFile? File { get; set; }

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.PendingSignature;
}
