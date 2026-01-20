using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LedgerViewModels;

public class EditAccountViewModel : UiStackLayoutViewModel
{
    public int Id { get; set; }

    public int EntityId { get; set; }

    [Required]
    [MaxLength(200)]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    public FinanceAccountType AccountType { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "CNY";

    public bool ShowInDashboard { get; set; }

    public bool IsArchived { get; set; }
}
