using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.LedgerViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Localization;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize(Policy = AppPermissionNames.CanManageLedger)]
[LimitPerMin]
public class LedgerController(
    EmployeeCenterDbContext dbContext,
    IStringLocalizer<LedgerController> localizer) : Controller
{
    [HttpGet]
    [RenderInNavBar(
        NavGroupName = "Administration",
        NavGroupOrder = 3,
        CascadedLinksGroupName = "Finance",
        CascadedLinksIcon = "shopping-cart",
        CascadedLinksOrder = 4,
        LinkText = "Manage Ledger",
        LinkOrder = 1)]
    public async Task<IActionResult> Index()
    {
        var entities = await dbContext.CompanyEntities
            .Where(t => t.CreateLedger)
            .OrderByDescending(t => t.CreationTime)
            .ToListAsync();
        var model = new IndexViewModel
        {
            Entities = entities,
            PageTitle = localizer["Manage Ledger"]
        };
        return this.StackView(model);
    }

    private async Task<Dictionary<string, decimal>> GetLatestExchangeRates(int entityId, string baseCurrency)
    {
        var rates = new Dictionary<string, decimal> { [baseCurrency] = 1 };
        var accounts = await dbContext.FinanceAccounts
            .Where(a => a.CompanyEntityId == entityId && !a.IsArchived)
            .ToListAsync();
        
        var currencies = accounts.Select(a => a.Currency).Distinct().Where(c => c != baseCurrency).ToList();
        
        foreach (var currency in currencies)
        {
            var rate = await dbContext.Transactions
                .Where(t => (t.SourceAccount!.Currency == currency && t.DestinationAccount!.Currency == baseCurrency) ||
                            (t.SourceAccount!.Currency == baseCurrency && t.DestinationAccount!.Currency == currency))
                .OrderByDescending(t => t.TransactionTime)
                .Select(t => t.SourceAccount!.Currency == currency ? t.ExchangeRate : 1 / t.ExchangeRate)
                .FirstOrDefaultAsync();
            rates[currency] = rate == 0 ? 1 : rate;
        }
        
        return rates;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(int id, int? accountId, int? year, int? month)
    {
        var entity = await dbContext.CompanyEntities.FindAsync(id);
        if (entity == null || !entity.CreateLedger)
        {
            return NotFound();
        }

        year ??= DateTime.UtcNow.Year;

        FinanceAccount? filteredAccount = null;
        if (accountId.HasValue)
        {
            filteredAccount = await dbContext.FinanceAccounts.FindAsync(accountId.Value);
            if (filteredAccount == null || filteredAccount.CompanyEntityId != id)
            {
                return NotFound();
            }
        }

        var allActiveAccounts = await dbContext.FinanceAccounts
            .Where(a => a.CompanyEntityId == id && !a.IsArchived)
            .ToListAsync();

        DateTime startTime;
        DateTime endTime;
        if (month.HasValue)
        {
            startTime = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            endTime = startTime.AddMonths(1);
        }
        else
        {
            startTime = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            endTime = startTime.AddYears(1);
        }

        // Optimize Balance Calculation (Avoid N+1)
        var sourceSums = await dbContext.Transactions
            .Where(t => t.SourceAccount!.CompanyEntityId == id && t.TransactionTime < endTime)
            .GroupBy(t => t.SourceAccountId)
            .Select(g => new { AccountId = g.Key, Sum = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.AccountId, x => x.Sum);

        var destinationSums = await dbContext.Transactions
            .Where(t => t.DestinationAccount!.CompanyEntityId == id && t.TransactionTime < endTime)
            .GroupBy(t => t.DestinationAccountId)
            .Select(g => new { AccountId = g.Key, Sum = g.Sum(t => t.Amount * t.ExchangeRate) })
            .ToDictionaryAsync(x => x.AccountId, x => x.Sum);

        var accountsWithBalance = allActiveAccounts.Select(a => new AccountWithBalance
        {
            Account = a,
            Balance = a.AccountType switch
            {
                FinanceAccountType.Asset or FinanceAccountType.Expense => (destinationSums.GetValueOrDefault(a.Id) - sourceSums.GetValueOrDefault(a.Id)),
                FinanceAccountType.Liability or FinanceAccountType.Equity or FinanceAccountType.Income => (sourceSums.GetValueOrDefault(a.Id) - destinationSums.GetValueOrDefault(a.Id)),
                _ => 0
            }
        }).ToList();

        var rates = await GetLatestExchangeRates(id, entity.BaseCurrency);

        var recentTransactionsQuery = dbContext.Transactions
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .Where(t => t.SourceAccount!.CompanyEntityId == id || t.DestinationAccount!.CompanyEntityId == id);

        if (accountId.HasValue)
        {
            recentTransactionsQuery = recentTransactionsQuery.Where(t => t.SourceAccountId == accountId || t.DestinationAccountId == accountId);
        }

        recentTransactionsQuery = recentTransactionsQuery.Where(t => t.TransactionTime >= startTime && t.TransactionTime < endTime);

        var recentTransactions = await recentTransactionsQuery
            .OrderByDescending(t => t.TransactionTime)
            .Take(100) // Limit for performance
            .ToListAsync();

        decimal totalInflow;
        decimal totalOutflow;
        var inflowDistribution = new Dictionary<string, decimal>();
        var outflowDistribution = new Dictionary<string, decimal>();

        if (accountId.HasValue)
        {
            totalInflow = recentTransactions
                .Where(t => t.DestinationAccountId == accountId)
                .Sum(t => t.Amount * t.ExchangeRate);
            totalOutflow = recentTransactions
                .Where(t => t.SourceAccountId == accountId)
                .Sum(t => t.Amount);

            inflowDistribution = recentTransactions
                .Where(t => t.DestinationAccountId == accountId)
                .GroupBy(t => t.SourceAccount?.AccountName ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount * t.ExchangeRate));

            outflowDistribution = recentTransactions
                .Where(t => t.SourceAccountId == accountId)
                .GroupBy(t => t.DestinationAccount?.AccountName ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
        }
        else
        {
            // Company-wide stats for the period (Converted to Base Currency)
            totalInflow = recentTransactions
                .Where(t => t.SourceAccount!.AccountType == FinanceAccountType.Income)
                .Sum(t => t.Amount * rates.GetValueOrDefault(t.SourceAccount!.Currency, 1));

            totalOutflow = recentTransactions
                .Where(t => t.DestinationAccount!.AccountType == FinanceAccountType.Expense)
                .Sum(t => t.Amount * t.ExchangeRate * rates.GetValueOrDefault(t.DestinationAccount!.Currency, 1));
        }

        // Chart Data (Always for the whole year)
        var chartInflow = new decimal[12];
        var chartOutflow = new decimal[12];
        var yearStart = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = yearStart.AddYears(1);

        // Optimize Chart Data (Database Aggregation)
        var yearInflowRows = await dbContext.Transactions
            .Where(t => t.SourceAccount!.CompanyEntityId == id && 
                        t.SourceAccount!.AccountType == FinanceAccountType.Income &&
                        t.TransactionTime >= yearStart && t.TransactionTime < yearEnd)
            .Select(t => new { t.TransactionTime, t.SourceAccount!.Currency, t.Amount })
            .AsNoTracking()
            .ToListAsync();

        var yearInflowByCurrency = yearInflowRows
            .GroupBy(t => new { t.TransactionTime.Month, t.Currency })
            .Select(g => new { g.Key.Month, g.Key.Currency, Sum = g.Sum(t => t.Amount) })
            .ToList();

        var yearOutflowRows = await dbContext.Transactions
            .Where(t => t.DestinationAccount!.CompanyEntityId == id && 
                        t.DestinationAccount!.AccountType == FinanceAccountType.Expense &&
                        t.TransactionTime >= yearStart && t.TransactionTime < yearEnd)
            .Select(t => new { t.TransactionTime, t.DestinationAccount!.Currency, ConvertedAmount = t.Amount * t.ExchangeRate })
            .AsNoTracking()
            .ToListAsync();

        var yearOutflowByCurrency = yearOutflowRows
            .GroupBy(t => new { t.TransactionTime.Month, t.Currency })
            .Select(g => new { g.Key.Month, g.Key.Currency, Sum = g.Sum(t => t.ConvertedAmount) })
            .ToList();

        if (accountId.HasValue)
        {
            // ... (keep account specific logic as it is already in SQL)
            var accountInflowRows = await dbContext.Transactions
                .Where(t => t.DestinationAccountId == accountId && t.TransactionTime >= yearStart && t.TransactionTime < yearEnd)
                .Select(t => new { t.TransactionTime, ConvertedAmount = t.Amount * t.ExchangeRate })
                .AsNoTracking()
                .ToListAsync();

            var accountInflow = accountInflowRows
                .GroupBy(t => t.TransactionTime.Month)
                .Select(g => new { Month = g.Key, Sum = g.Sum(t => t.ConvertedAmount) })
                .ToList();
            
            var accountOutflowRows = await dbContext.Transactions
                .Where(t => t.SourceAccountId == accountId && t.TransactionTime >= yearStart && t.TransactionTime < yearEnd)
                .Select(t => new { t.TransactionTime, t.Amount })
                .AsNoTracking()
                .ToListAsync();

            var accountOutflow = accountOutflowRows
                .GroupBy(t => t.TransactionTime.Month)
                .Select(g => new { Month = g.Key, Sum = g.Sum(t => t.Amount) })
                .ToList();

            foreach (var d in accountInflow) chartInflow[d.Month - 1] = d.Sum;
            foreach (var d in accountOutflow) chartOutflow[d.Month - 1] = d.Sum;
        }
        else
        {
            foreach (var d in yearInflowByCurrency)
            {
                chartInflow[d.Month - 1] += d.Sum * rates.GetValueOrDefault(d.Currency, 1);
            }
            foreach (var d in yearOutflowByCurrency)
            {
                chartOutflow[d.Month - 1] += d.Sum * rates.GetValueOrDefault(d.Currency, 1);
            }
        }

        // Company-wide snapshot (Converted to Base Currency)
        var totalAssets = accountsWithBalance
            .Where(a => a.Account.AccountType == FinanceAccountType.Asset)
            .Sum(a => a.Balance * rates.GetValueOrDefault(a.Account.Currency, 1));
        var totalLiabilities = accountsWithBalance
            .Where(a => a.Account.AccountType == FinanceAccountType.Liability)
            .Sum(a => a.Balance * rates.GetValueOrDefault(a.Account.Currency, 1));
        var totalEquity = accountsWithBalance
            .Where(a => a.Account.AccountType == FinanceAccountType.Equity)
            .Sum(a => a.Balance * rates.GetValueOrDefault(a.Account.Currency, 1));

        // Calculate Burn Rate (Expense in last 30 days)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var burnByCurrency = await dbContext.Transactions
            .Where(t => t.DestinationAccount!.CompanyEntityId == id &&
                        t.DestinationAccount!.AccountType == FinanceAccountType.Expense &&
                        t.TransactionTime >= thirtyDaysAgo)
            .GroupBy(t => t.DestinationAccount!.Currency)
            .Select(g => new { Currency = g.Key, Sum = g.Sum(t => t.Amount * t.ExchangeRate) })
            .ToListAsync();
        
        var totalBurn = burnByCurrency.Sum(b => b.Sum * rates.GetValueOrDefault(b.Currency, 1));

        // Calculate Runway: Total Assets / Burn Rate
        decimal totalAssetsToRun;
        if (accountId.HasValue)
        {
            var account = accountsWithBalance.First(a => a.Account.Id == accountId.Value);
            totalAssetsToRun = account.Balance * (account.Account.AccountType == FinanceAccountType.Asset ? 1 : 0); // Only assets contribute to runway
        }
        else
        {
            totalAssetsToRun = totalAssets;
        }

        var model = new DashboardViewModel
        {
            Entity = entity,
            Accounts = accountsWithBalance.Where(a => a.Account.ShowInDashboard).ToList(),
            RecentTransactions = recentTransactions,
            MonthlyBurnRate = totalBurn,
            RunwayMonths = totalBurn > 0 ? totalAssetsToRun / totalBurn : null,
            FilteredAccount = filteredAccount,
            FilteredAccountBalance = accountId.HasValue ? accountsWithBalance.First(a => a.Account.Id == accountId).Balance : 0,
            Year = year.Value,
            Month = month,
            TotalInflow = totalInflow,
            TotalOutflow = totalOutflow,
            ChartLabels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
            ChartInflowData = chartInflow,
            ChartOutflowData = chartOutflow,
            InflowDistribution = inflowDistribution,
            OutflowDistribution = outflowDistribution,
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            TotalEquity = totalEquity,
            RevenueForPeriod = totalInflow,
            ExpensesForPeriod = totalOutflow,
            NetIncomeForPeriod = totalInflow - totalOutflow,
            PageTitle = $"{entity.CompanyName} - {(filteredAccount != null ? filteredAccount.AccountName : localizer["Ledger Dashboard"])}"
        };

        return this.StackView(model);
    }

    [HttpGet]
    public async Task<IActionResult> Accounts(int id)
    {
        var entity = await dbContext.CompanyEntities.FindAsync(id);
        if (entity == null || !entity.CreateLedger)
        {
            return NotFound();
        }

        var accounts = await dbContext.FinanceAccounts
            .Where(a => a.CompanyEntityId == id)
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.AccountName)
            .ToListAsync();

        var sourceSums = await dbContext.Transactions
            .Where(t => t.SourceAccount!.CompanyEntityId == id)
            .GroupBy(t => t.SourceAccountId)
            .Select(g => new { AccountId = g.Key, Sum = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.AccountId, x => x.Sum);

        var destinationSums = await dbContext.Transactions
            .Where(t => t.DestinationAccount!.CompanyEntityId == id)
            .GroupBy(t => t.DestinationAccountId)
            .Select(g => new { AccountId = g.Key, Sum = g.Sum(t => t.Amount * t.ExchangeRate) })
            .ToDictionaryAsync(x => x.AccountId, x => x.Sum);

        var accountsWithBalance = accounts.Select(a => new AccountWithBalance
        {
            Account = a,
            Balance = a.AccountType switch
            {
                FinanceAccountType.Asset or FinanceAccountType.Expense => (destinationSums.GetValueOrDefault(a.Id) - sourceSums.GetValueOrDefault(a.Id)),
                FinanceAccountType.Liability or FinanceAccountType.Equity or FinanceAccountType.Income => (sourceSums.GetValueOrDefault(a.Id) - destinationSums.GetValueOrDefault(a.Id)),
                _ => 0
            }
        }).ToList();

        var model = new AccountsViewModel
        {
            EntityId = id,
            EntityName = entity.CompanyName,
            Accounts = accountsWithBalance,
            PageTitle = $"{entity.CompanyName} - {localizer["Manage Accounts"]}"
        };
        return this.StackView(model);
    }

    [HttpGet]
    public IActionResult CreateAccount(int id)
    {
        return this.StackView(new CreateAccountViewModel
        {
            EntityId = id,
            PageTitle = localizer["Create New Account"]
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccount(CreateAccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.PageTitle = localizer["Create New Account"];
            return this.StackView(model);
        }

        var account = new FinanceAccount
        {
            AccountName = model.AccountName,
            AccountType = model.AccountType,
            CompanyEntityId = model.EntityId,
            Currency = model.Currency,
            ShowInDashboard = model.ShowInDashboard
        };

        dbContext.FinanceAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Accounts), new { id = model.EntityId });
    }

    [HttpGet]
    public async Task<IActionResult> EditAccount(int id)
    {
        var account = await dbContext.FinanceAccounts.FindAsync(id);
        if (account == null)
        {
            return NotFound();
        }

        var model = new EditAccountViewModel
        {
            Id = account.Id,
            EntityId = account.CompanyEntityId,
            AccountName = account.AccountName,
            AccountType = account.AccountType,
            Currency = account.Currency,
            ShowInDashboard = account.ShowInDashboard,
            IsArchived = account.IsArchived,
            PageTitle = $"{localizer["Edit Account"]} - {account.AccountName}"
        };

        return this.StackView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAccount(EditAccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.PageTitle = $"{localizer["Edit Account"]} - {model.AccountName}";
            return this.StackView(model);
        }

        var account = await dbContext.FinanceAccounts.FindAsync(model.Id);
        if (account == null)
        {
            return NotFound();
        }

        account.AccountName = model.AccountName;
        account.AccountType = model.AccountType;
        account.Currency = model.Currency;
        account.ShowInDashboard = model.ShowInDashboard;
        account.IsArchived = model.IsArchived;

        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Accounts), new { id = account.CompanyEntityId });
    }

    [HttpGet]
    public async Task<IActionResult> Transactions(int id)
    {
        var entity = await dbContext.CompanyEntities.FindAsync(id);
        if (entity == null || !entity.CreateLedger)
        {
            return NotFound();
        }

        var transactions = await dbContext.Transactions
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .Where(t => t.SourceAccount!.CompanyEntityId == id || t.DestinationAccount!.CompanyEntityId == id)
            .OrderByDescending(t => t.TransactionTime)
            .ToListAsync();

        var model = new TransactionsViewModel
        {
            Entity = entity,
            Transactions = transactions,
            PageTitle = $"{entity.CompanyName} - {localizer["Transactions"]}"
        };

        return this.StackView(model);
    }

    [HttpGet]
    public async Task<IActionResult> CreateTransaction(int id)
    {
        var accounts = await dbContext.FinanceAccounts
            .Where(a => a.CompanyEntityId == id && !a.IsArchived)
            .ToListAsync();

        // Also allow selecting accounts from other entities for inter-entity transactions?
        // User said "一人掌控两地公司", so maybe.
        // But for MVP, let's stick to accounts of the current entity + maybe global accounts if any.
        // Actually, "Anduin's Wallet" might be shared or per-entity.
        // Requirement says "Belongs To Entity" for accounts.

        ViewBag.Accounts = accounts;
        return this.StackView(new CreateTransactionViewModel
        {
            EntityId = id,
            TransactionTime = DateTime.UtcNow,
            PageTitle = localizer["New Transaction"]
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTransaction(CreateTransactionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Accounts = await dbContext.FinanceAccounts
                .Where(a => a.CompanyEntityId == model.EntityId && !a.IsArchived)
                .ToListAsync();
            model.PageTitle = localizer["New Transaction"];
            return this.StackView(model);
        }

        var sourceAccount = await dbContext.FinanceAccounts.FindAsync(model.SourceAccountId);
        var destinationAccount = await dbContext.FinanceAccounts.FindAsync(model.DestinationAccountId);

        if (sourceAccount == null || destinationAccount == null)
        {
            return NotFound();
        }

        if (sourceAccount.CompanyEntityId != model.EntityId && destinationAccount.CompanyEntityId != model.EntityId)
        {
            ModelState.AddModelError(string.Empty, "At least one of the accounts must belong to the current entity.");
            ViewBag.Accounts = await dbContext.FinanceAccounts
                .Where(a => a.CompanyEntityId == model.EntityId && !a.IsArchived)
                .ToListAsync();
            model.PageTitle = localizer["New Transaction"];
            return this.StackView(model);
        }

        var transaction = new Transaction
        {
            Description = model.Description,
            SourceAccountId = model.SourceAccountId,
            DestinationAccountId = model.DestinationAccountId,
            Amount = model.Amount,
            ExchangeRate = model.ExchangeRate,
            InvoicePath = model.InvoicePath,
            MT103Path = model.MT103Path,
            PaymentVoucherPath = model.PaymentVoucherPath,
            TransactionTime = model.TransactionTime
        };

        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Dashboard), new { id = model.EntityId });
    }

    [HttpGet]
    public async Task<IActionResult> EditTransaction(int id, int entityId)
    {
        var transaction = await dbContext.Transactions.FindAsync(id);
        if (transaction == null)
        {
            return NotFound();
        }

        var accounts = await dbContext.FinanceAccounts
            .Where(a => a.CompanyEntityId == entityId && !a.IsArchived)
            .ToListAsync();

        ViewBag.Accounts = accounts;
        var model = new EditTransactionViewModel
        {
            EntityId = entityId,
            TransactionId = transaction.Id,
            Description = transaction.Description,
            SourceAccountId = transaction.SourceAccountId,
            DestinationAccountId = transaction.DestinationAccountId,
            Amount = transaction.Amount,
            ExchangeRate = transaction.ExchangeRate,
            InvoicePath = transaction.InvoicePath,
            MT103Path = transaction.MT103Path,
            PaymentVoucherPath = transaction.PaymentVoucherPath,
            TransactionTime = transaction.TransactionTime,
            PageTitle = localizer["Edit Transaction"]
        };

        return this.StackView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTransaction(EditTransactionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Accounts = await dbContext.FinanceAccounts
                .Where(a => a.CompanyEntityId == model.EntityId && !a.IsArchived)
                .ToListAsync();
            model.PageTitle = localizer["Edit Transaction"];
            return this.StackView(model);
        }

        var transaction = await dbContext.Transactions.FindAsync(model.TransactionId);
        if (transaction == null)
        {
            return NotFound();
        }

        transaction.Description = model.Description;
        transaction.SourceAccountId = model.SourceAccountId;
        transaction.DestinationAccountId = model.DestinationAccountId;
        transaction.Amount = model.Amount;
        transaction.ExchangeRate = model.ExchangeRate;
        transaction.InvoicePath = model.InvoicePath;
        transaction.MT103Path = model.MT103Path;
        transaction.PaymentVoucherPath = model.PaymentVoucherPath;
        transaction.TransactionTime = model.TransactionTime;

        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Dashboard), new { id = model.EntityId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTransaction(int id, int entityId)
    {
        var transaction = await dbContext.Transactions.FindAsync(id);
        if (transaction == null)
        {
            return NotFound();
        }

        dbContext.Transactions.Remove(transaction);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Dashboard), new { id = entityId });
    }
}
