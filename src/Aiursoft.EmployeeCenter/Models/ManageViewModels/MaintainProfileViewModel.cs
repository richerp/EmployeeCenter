using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ManageViewModels;

public class MaintainProfileViewModel : UiStackLayoutViewModel
{
    public MaintainProfileViewModel()
    {
        PageTitle = "Maintain Profile";
    }

    [Display(Name = "Legal Name")]
    [MaxLength(100)]
    public string? LegalName { get; set; }

    [Display(Name = "Phone Number")]
    [Phone]
    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Bank Account (Card Number)")]
    [MaxLength(30)]
    public string? BankAccount { get; set; }

    [Display(Name = "Bank Name (Branch)")]
    [MaxLength(30)]
    public string? BankName { get; set; }

    [Display(Name = "Bank Account Name (Holder)")]
    [MaxLength(30)]
    public string? BankAccountName { get; set; }
}
