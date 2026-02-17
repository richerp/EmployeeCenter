using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LedgerViewModels;

public class AccountWithBalance
{
    [Display(Name = "Account")]
    public required FinanceAccount Account { get; set; }

    [Display(Name = "Balance")]
    public decimal Balance { get; set; }
}

public class DashboardViewModel : UiStackLayoutViewModel
{
    [Display(Name = "Entity")]
    public required CompanyEntity Entity { get; set; }

    [Display(Name = "Accounts")]
    public List<AccountWithBalance> Accounts { get; set; } = new();

    [Display(Name = "Recent Transactions")]
    public List<Transaction> RecentTransactions { get; set; } = new();

    [Display(Name = "Monthly Burn Rate")]
    public decimal MonthlyBurnRate { get; set; }

    [Display(Name = "Runway (Months)")]
    public decimal? RunwayMonths { get; set; }

    [Display(Name = "Filtered Account")]
    public FinanceAccount? FilteredAccount { get; set; }

    [Display(Name = "Filtered Account Balance")]
    public decimal FilteredAccountBalance { get; set; }
    
    [Display(Name = "Year")]
    public int Year { get; set; }

    [Display(Name = "Month")]
    public int? Month { get; set; }
    
    [Display(Name = "Total Inflow")]
    public decimal TotalInflow { get; set; }

    [Display(Name = "Total Outflow")]
    public decimal TotalOutflow { get; set; }
    
    [Display(Name = "Chart Labels")]
    public string[] ChartLabels { get; set; } = [];

    [Display(Name = "Chart Inflow Data")]
    public decimal[] ChartInflowData { get; set; } = [];

    [Display(Name = "Chart Outflow Data")]
    public decimal[] ChartOutflowData { get; set; } = [];

    [Display(Name = "Inflow Distribution")]
    public Dictionary<string, decimal> InflowDistribution { get; set; } = new();

    [Display(Name = "Outflow Distribution")]
    public Dictionary<string, decimal> OutflowDistribution { get; set; } = new();
}
