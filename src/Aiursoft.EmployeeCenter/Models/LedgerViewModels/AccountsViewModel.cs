using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LedgerViewModels;

public class AccountsViewModel : UiStackLayoutViewModel
{
    public int EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public List<AccountWithBalance> Accounts { get; set; } = new();
}
