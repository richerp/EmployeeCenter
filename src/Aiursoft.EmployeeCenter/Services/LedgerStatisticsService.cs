using Aiursoft.EmployeeCenter.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Services;

/// <summary>
/// Service for computing ledger statistics: monthly chart data, flow summaries,
/// burn rate, runway, and inflow/outflow distributions.
/// </summary>
public class LedgerStatisticsService(EmployeeCenterDbContext dbContext)
{
    // ────────────────────────────────────────────
    //  Monthly chart data (line chart – 12 months)
    // ────────────────────────────────────────────

    /// <summary>
    /// Returns monthly inflow/outflow arrays (12 elements each) for the given year.
    /// When <paramref name="accountId"/> is specified, inflow = money entering that account,
    /// outflow = money leaving that account.
    /// When <paramref name="accountId"/> is null, inflow = company-wide Income,
    /// outflow = company-wide Expense, both converted to base currency via <paramref name="rates"/>.
    /// </summary>
    public async Task<MonthlyChartData> GetMonthlyChartDataAsync(
        int entityId, int? accountId, int year, Dictionary<string, decimal> rates)
    {
        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = yearStart.AddYears(1);

        var chartInflow = new decimal[12];
        var chartOutflow = new decimal[12];

        if (accountId.HasValue)
        {
            var inflowRows = await dbContext.Transactions
                .Where(t => t.DestinationAccountId == accountId && t.TransactionTime >= yearStart && t.TransactionTime < yearEnd)
                .Select(t => new { t.TransactionTime, ConvertedAmount = t.Amount * t.ExchangeRate })
                .AsNoTracking()
                .ToListAsync();

            foreach (var g in inflowRows.GroupBy(t => t.TransactionTime.Month))
                chartInflow[g.Key - 1] = g.Sum(t => t.ConvertedAmount);

            var outflowRows = await dbContext.Transactions
                .Where(t => t.SourceAccountId == accountId && t.TransactionTime >= yearStart && t.TransactionTime < yearEnd)
                .Select(t => new { t.TransactionTime, t.Amount })
                .AsNoTracking()
                .ToListAsync();

            foreach (var g in outflowRows.GroupBy(t => t.TransactionTime.Month))
                chartOutflow[g.Key - 1] = g.Sum(t => t.Amount);
        }
        else
        {
            var inflowRows = await dbContext.Transactions
                .Where(t => t.SourceAccount!.CompanyEntityId == entityId &&
                            t.SourceAccount!.AccountType == FinanceAccountType.Income &&
                            t.TransactionTime >= yearStart && t.TransactionTime < yearEnd)
                .Select(t => new { t.TransactionTime, t.SourceAccount!.Currency, t.Amount })
                .AsNoTracking()
                .ToListAsync();

            foreach (var g in inflowRows.GroupBy(t => new { t.TransactionTime.Month, t.Currency }))
                chartInflow[g.Key.Month - 1] += g.Sum(t => t.Amount) * rates.GetValueOrDefault(g.Key.Currency, 1);

            var outflowRows = await dbContext.Transactions
                .Where(t => t.DestinationAccount!.CompanyEntityId == entityId &&
                            t.DestinationAccount!.AccountType == FinanceAccountType.Expense &&
                            t.TransactionTime >= yearStart && t.TransactionTime < yearEnd)
                .Select(t => new { t.TransactionTime, t.DestinationAccount!.Currency, ConvertedAmount = t.Amount * t.ExchangeRate })
                .AsNoTracking()
                .ToListAsync();

            foreach (var g in outflowRows.GroupBy(t => new { t.TransactionTime.Month, t.Currency }))
                chartOutflow[g.Key.Month - 1] += g.Sum(t => t.ConvertedAmount) * rates.GetValueOrDefault(g.Key.Currency, 1);
        }

        return new MonthlyChartData
        {
            Labels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
            InflowData = chartInflow,
            OutflowData = chartOutflow,
        };
    }

    // ────────────────────────────────────
    //  Flow summary (total inflow/outflow)
    // ────────────────────────────────────

    /// <summary>
    /// Computes total inflow and outflow for a period.
    /// For a specific account: inflow = money entering, outflow = money leaving.
    /// For company-wide: inflow = Income-type outflows, outflow = Expense-type inflows,
    /// both converted to base currency.
    /// </summary>
    public async Task<FlowSummary> GetFlowSummaryAsync(
        int entityId, int? accountId, DateTime startTime, DateTime endTime, Dictionary<string, decimal> rates)
    {
        var recentTransactions = await GetRecentTransactionsQueryBase(entityId, accountId, startTime, endTime)
            .OrderByDescending(t => t.TransactionTime)
            .Take(100)
            .ToListAsync();

        decimal totalInflow;
        decimal totalOutflow;

        if (accountId.HasValue)
        {
            totalInflow = recentTransactions
                .Where(t => t.DestinationAccountId == accountId)
                .Sum(t => t.Amount * t.ExchangeRate);
            totalOutflow = recentTransactions
                .Where(t => t.SourceAccountId == accountId)
                .Sum(t => t.Amount);
        }
        else
        {
            totalInflow = recentTransactions
                .Where(t => t.SourceAccount!.AccountType == FinanceAccountType.Income)
                .Sum(t => t.Amount * rates.GetValueOrDefault(t.SourceAccount!.Currency, 1));

            totalOutflow = recentTransactions
                .Where(t => t.DestinationAccount!.AccountType == FinanceAccountType.Expense)
                .Sum(t => t.Amount * t.ExchangeRate * rates.GetValueOrDefault(t.DestinationAccount!.Currency, 1));
        }

        return new FlowSummary
        {
            TotalInflow = totalInflow,
            TotalOutflow = totalOutflow,
        };
    }

    // ────────────────────────────
    //  Recent transactions
    // ────────────────────────────

    /// <summary>
    /// Returns the most recent transactions for the given entity/account within the time range.
    /// </summary>
    public async Task<List<Transaction>> GetRecentTransactionsAsync(
        int entityId, int? accountId, DateTime startTime, DateTime endTime, int limit = 100)
    {
        return await GetRecentTransactionsQueryBase(entityId, accountId, startTime, endTime)
            .OrderByDescending(t => t.TransactionTime)
            .Take(limit)
            .ToListAsync();
    }

    // ────────────────────────────────────
    //  Burn rate & runway
    // ────────────────────────────────────

    /// <summary>
    /// Calculates the monthly burn rate (total Expense in the last 30 days), converted to base currency.
    /// </summary>
    public async Task<decimal> GetBurnRateAsync(int entityId, Dictionary<string, decimal> rates)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var burnByCurrency = await dbContext.Transactions
            .Where(t => t.DestinationAccount!.CompanyEntityId == entityId &&
                        t.DestinationAccount!.AccountType == FinanceAccountType.Expense &&
                        t.TransactionTime >= thirtyDaysAgo)
            .GroupBy(t => t.DestinationAccount!.Currency)
            .Select(g => new { Currency = g.Key, Sum = g.Sum(t => t.Amount * t.ExchangeRate) })
            .ToListAsync();

        return burnByCurrency.Sum(b => b.Sum * rates.GetValueOrDefault(b.Currency, 1));
    }

    /// <summary>
    /// Calculates runway in months: totalAssets / burnRate.
    /// Returns null when burn rate is zero (infinite runway).
    /// </summary>
    public static decimal? CalculateRunway(decimal totalAssetsToRun, decimal burnRate)
    {
        return burnRate > 0 ? totalAssetsToRun / burnRate : null;
    }

    // ──────────────────────────────────────
    //  Inflow / outflow distribution (pie)
    // ──────────────────────────────────────

    /// <summary>
    /// Returns the inflow and outflow distributions for a specific account, grouped by counterparty account name.
    /// Only meaningful when a specific account is selected.
    /// </summary>
    public async Task<FlowDistribution> GetFlowDistributionAsync(
        int entityId, int accountId, DateTime startTime, DateTime endTime)
    {
        var transactions = await GetRecentTransactionsQueryBase(entityId, accountId, startTime, endTime)
            .OrderByDescending(t => t.TransactionTime)
            .Take(100)
            .ToListAsync();

        return new FlowDistribution
        {
            InflowDistribution = transactions
                .Where(t => t.DestinationAccountId == accountId)
                .GroupBy(t => t.SourceAccount?.AccountName ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount * t.ExchangeRate)),

            OutflowDistribution = transactions
                .Where(t => t.SourceAccountId == accountId)
                .GroupBy(t => t.DestinationAccount?.AccountName ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount)),
        };
    }

    // ──────────────────
    //  Private helpers
    // ──────────────────

    private IQueryable<Transaction> GetRecentTransactionsQueryBase(
        int entityId, int? accountId, DateTime startTime, DateTime endTime)
    {
        var query = dbContext.Transactions
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .Where(t => t.SourceAccount!.CompanyEntityId == entityId || t.DestinationAccount!.CompanyEntityId == entityId);

        if (accountId.HasValue)
        {
            query = query.Where(t => t.SourceAccountId == accountId || t.DestinationAccountId == accountId);
        }

        query = query.Where(t => t.TransactionTime >= startTime && t.TransactionTime < endTime);

        return query;
    }
}

// ──────────────
//  Result DTOs
// ──────────────

public class MonthlyChartData
{
    public required string[] Labels { get; init; }
    public required decimal[] InflowData { get; init; }
    public required decimal[] OutflowData { get; init; }
}

public class FlowSummary
{
    public decimal TotalInflow { get; init; }
    public decimal TotalOutflow { get; init; }
}

public class FlowDistribution
{
    public Dictionary<string, decimal> InflowDistribution { get; init; } = new();
    public Dictionary<string, decimal> OutflowDistribution { get; init; } = new();
}
