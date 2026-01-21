using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.LedgerViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize(Policy = AppPermissionNames.CanManageLedger)]
[LimitPerMin]
public class LedgerController(
    EmployeeCenterDbContext dbContext) : Controller
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
            .OrderByDescending(t => t.CreationTime)
            .ToListAsync();
        var model = new IndexViewModel
        {
            Entities = entities
        };
        return this.StackView(model);
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(int id, int? accountId)
    {
        var entity = await dbContext.CompanyEntities.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        FinanceAccount? filteredAccount = null;
        if (accountId.HasValue)
        {
            filteredAccount = await dbContext.FinanceAccounts.FindAsync(accountId.Value);
            if (filteredAccount == null || filteredAccount.CompanyEntityId != id)
            {
                return NotFound();
            }
        }

        var accounts = await dbContext.FinanceAccounts
            .Where(a => a.CompanyEntityId == id && !a.IsArchived && a.ShowInDashboard)
            .ToListAsync();


        var accountsWithBalance = new List<AccountWithBalance>();
        foreach (var account in accounts)
        {
            accountsWithBalance.Add(new AccountWithBalance
            {
                Account = account,
                Balance = await GetBalance(account.Id)
            });
        }

        var recentTransactionsQuery = dbContext.Transactions
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .Where(t => t.SourceAccount!.CompanyEntityId == id || t.DestinationAccount!.CompanyEntityId == id);

        if (accountId.HasValue)
        {
            recentTransactionsQuery = recentTransactionsQuery.Where(t => t.SourceAccountId == accountId || t.DestinationAccountId == accountId);
        }

        var recentTransactions = await recentTransactionsQuery
            .OrderByDescending(t => t.TransactionTime)
            .Take(accountId.HasValue ? 1000 : 10)
            .ToListAsync();

        // Calculate Burn Rate (Expense in last 30 days)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var monthlyExpensesQuery = dbContext.Transactions
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .Where(t => t.DestinationAccount!.CompanyEntityId == id && 
                        t.DestinationAccount!.AccountType == FinanceAccountType.Expense &&
                        t.TransactionTime >= thirtyDaysAgo);

        if (accountId.HasValue)
        {
            monthlyExpensesQuery = monthlyExpensesQuery.Where(t => t.SourceAccountId == accountId);
        }

        var monthlyExpenses = await monthlyExpensesQuery.ToListAsync();
        
        // This is tricky because expenses can be in different currencies. 
        // For now, we just sum them up if they match entity's base currency, 
        // or provide a simple sum in base currency if we had more exchange rate info.
        // Let's assume for MVP we sum expenses that ended up in an Expense account of this entity.
        // Since the DestinationAccount belongs to this entity, the amount is Amount * ExchangeRate in Destination Currency.
        // We should ideally convert everything to Entity.BaseCurrency.
        
        decimal totalBurn = 0;
        foreach(var exp in monthlyExpenses)
        {
            // If expense account currency matches entity base currency, just add.
            if (exp.DestinationAccount!.Currency == entity.BaseCurrency)
            {
                totalBurn += exp.Amount * exp.ExchangeRate;
            }
            // Else, we might need another exchange rate to base currency... 
            // For MVP, let's just sum what we can.
        }

        // Calculate Runway: Total Assets / Burn Rate
        decimal totalAssets;
        if (accountId.HasValue)
        {
            totalAssets = await GetBalance(accountId.Value);
        }
        else
        {
            totalAssets = accountsWithBalance
                .Where(a => a.Account.AccountType == FinanceAccountType.Asset && a.Account.Currency == entity.BaseCurrency)
                .Sum(a => a.Balance);
        }

        var model = new DashboardViewModel
        {
            Entity = entity,
            Accounts = accountsWithBalance,
            RecentTransactions = recentTransactions,
            MonthlyBurnRate = totalBurn,
            RunwayMonths = totalBurn > 0 ? totalAssets / totalBurn : null,
            FilteredAccount = filteredAccount,
            FilteredAccountBalance = totalAssets
        };

        return this.StackView(model);
    }

    [HttpGet]
    public async Task<IActionResult> Accounts(int id)
    {
        var entity = await dbContext.CompanyEntities.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        var accounts = await dbContext.FinanceAccounts
            .Where(a => a.CompanyEntityId == id)
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.AccountName)
            .ToListAsync();

        var accountsWithBalance = new List<AccountWithBalance>();
        foreach (var account in accounts)
        {
            accountsWithBalance.Add(new AccountWithBalance
            {
                Account = account,
                Balance = await GetBalance(account.Id)
            });
        }

        var model = new AccountsViewModel
        {
            EntityId = id,
            EntityName = entity.CompanyName,
            Accounts = accountsWithBalance
        };
        return this.StackView(model);
    }

    [HttpGet]
    public IActionResult CreateAccount(int id)
    {
        return this.StackView(new CreateAccountViewModel
        {
            EntityId = id
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccount(CreateAccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
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
            IsArchived = account.IsArchived
        };

        return this.StackView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAccount(EditAccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
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
        if (entity == null)
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
            Transactions = transactions
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
            TransactionTime = DateTime.UtcNow
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
            TransactionTime = transaction.TransactionTime
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

    private async Task<decimal> GetBalance(int accountId)
    {
        var account = await dbContext.FinanceAccounts.FindAsync(accountId);
        if (account == null) return 0;

        var sourceSum = await dbContext.Transactions
            .Where(t => t.SourceAccountId == accountId)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var destinationSum = await dbContext.Transactions
            .Where(t => t.DestinationAccountId == accountId)
            .SumAsync(t => (decimal?)t.Amount * t.ExchangeRate) ?? 0;

        return account.AccountType switch
        {
            FinanceAccountType.Asset or FinanceAccountType.Expense => destinationSum - sourceSum,
            FinanceAccountType.Liability or FinanceAccountType.Equity or FinanceAccountType.Income => sourceSum - destinationSum,
            _ => 0
        };
    }
}
