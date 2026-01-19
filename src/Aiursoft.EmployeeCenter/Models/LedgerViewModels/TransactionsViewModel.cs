using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LedgerViewModels;

public class TransactionsViewModel : UiStackLayoutViewModel
{
    public required CompanyEntity Entity { get; set; }
    public List<Transaction> Transactions { get; set; } = new();
}
