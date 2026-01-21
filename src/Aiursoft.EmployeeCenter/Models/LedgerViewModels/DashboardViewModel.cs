using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LedgerViewModels;

public class AccountWithBalance
{
    public required FinanceAccount Account { get; set; }
    public decimal Balance { get; set; }
}

public class DashboardViewModel : UiStackLayoutViewModel
{
    public required CompanyEntity Entity { get; set; }
    public List<AccountWithBalance> Accounts { get; set; } = new();
    public List<Transaction> RecentTransactions { get; set; } = new();
    public decimal MonthlyBurnRate { get; set; }
    public decimal? RunwayMonths { get; set; }
    public FinanceAccount? FilteredAccount { get; set; }
}
