using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ManageViewModels;

public class ChangeBankInfoViewModel : UiStackLayoutViewModel
{
    public ChangeBankInfoViewModel()
    {
        PageTitle = "Change Bank Information";
    }

    [Display(Name = "Bank Card Number")]
    [MaxLength(30)]
    public string? BankCardNumber { get; set; }

    [Display(Name = "Bank Name")]
    [MaxLength(30)]
    public string? BankName { get; set; }

    [Display(Name = "Bank Account Name")]
    [MaxLength(30)]
    public string? BankAccountName { get; set; }
}
