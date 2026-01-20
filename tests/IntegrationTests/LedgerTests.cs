namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class LedgerTests : TestBase
{
    [TestMethod]
    public async Task TestLedgerFlow()
    {
        // 1. Setup - Create an entity
        int entityId;
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var entity = new CompanyEntity
            {
                CompanyName = "Test Entity",
                EntityCode = "TE001",
                BaseCurrency = "HKD"
            };
            db.CompanyEntities.Add(entity);
            await db.SaveChangesAsync();
            entityId = entity.Id;
        }

        // 2. Login as Admin
        await LoginAsAdmin();

        // 3. Create Accounts
        var createAccountResponse = await PostForm("/Ledger/CreateAccount", new Dictionary<string, string>
        {
            { "EntityId", entityId.ToString() },
            { "AccountName", "Bank" },
            { "AccountType", FinanceAccountType.Asset.ToString() },
            { "Currency", "HKD" }
        });
        AssertRedirect(createAccountResponse, "/Ledger/Accounts/" + entityId);

        var createAccountResponse2 = await PostForm("/Ledger/CreateAccount", new Dictionary<string, string>
        {
            { "EntityId", entityId.ToString() },
            { "AccountName", "Capital" },
            { "AccountType", FinanceAccountType.Equity.ToString() },
            { "Currency", "HKD" }
        });
        AssertRedirect(createAccountResponse2, "/Ledger/Accounts/" + entityId);

        int bankId, capitalId;
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            bankId = db.FinanceAccounts.First(a => a.AccountName == "Bank").Id;
            capitalId = db.FinanceAccounts.First(a => a.AccountName == "Capital").Id;
        }

        // 4. Create Transaction (Injection)
        var createTransactionResponse = await PostForm("/Ledger/CreateTransaction", new Dictionary<string, string>
        {
            { "EntityId", entityId.ToString() },
            { "Description", "Initial Capital" },
            { "SourceAccountId", capitalId.ToString() },
            { "DestinationAccountId", bankId.ToString() },
            { "Amount", "10000" },
            { "ExchangeRate", "1" },
            { "TransactionTime", DateTime.UtcNow.ToString("O") }
        });
        AssertRedirect(createTransactionResponse, "/Ledger/Dashboard/" + entityId);

        // 5. Verify Balance
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            
            // Asset: Destination - Source = 10000 - 0 = 10000
            var bankBalance = await GetBalance(db, bankId);
            Assert.AreEqual(10000, bankBalance);

            // Equity: Source - Destination = 10000 - 0 = 10000
            var capitalBalance = await GetBalance(db, capitalId);
            Assert.AreEqual(10000, capitalBalance);
        }

        // 6. Create Expense Transaction
        int expenseAccountId;
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var expenseAccount = new FinanceAccount
            {
                AccountName = "Legal Fee",
                AccountType = FinanceAccountType.Expense,
                CompanyEntityId = entityId,
                Currency = "HKD"
            };
            db.FinanceAccounts.Add(expenseAccount);
            await db.SaveChangesAsync();
            expenseAccountId = expenseAccount.Id;
        }

        var createExpenseResponse = await PostForm("/Ledger/CreateTransaction", new Dictionary<string, string>
        {
            { "EntityId", entityId.ToString() },
            { "Description", "Sleek fee" },
            { "SourceAccountId", bankId.ToString() },
            { "DestinationAccountId", expenseAccountId.ToString() },
            { "Amount", "3000" },
            { "ExchangeRate", "1" },
            { "TransactionTime", DateTime.UtcNow.ToString("O") }
        });
        AssertRedirect(createExpenseResponse, "/Ledger/Dashboard/" + entityId);

        // 7. Verify Final Balances
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            
            // Bank: 10000 - 3000 = 7000
            Assert.AreEqual(7000, await GetBalance(db, bankId));
            
            // Expense: 3000 - 0 = 3000
            Assert.AreEqual(3000, await GetBalance(db, expenseAccountId));
        }
    }

    [TestMethod]
    public async Task TestAccountEditAndDashboardFiltering()
    {
        // 1. Setup - Create an entity and an account
        int entityId;
        int accountId;
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var entity = new CompanyEntity
            {
                CompanyName = "Filter Test Entity",
                EntityCode = "FTE01",
                BaseCurrency = "USD"
            };
            db.CompanyEntities.Add(entity);
            await db.SaveChangesAsync();
            entityId = entity.Id;

            var account = new FinanceAccount
            {
                AccountName = "Initial Name",
                AccountType = FinanceAccountType.Asset,
                CompanyEntityId = entityId,
                Currency = "USD",
                ShowInDashboard = true
            };
            db.FinanceAccounts.Add(account);
            await db.SaveChangesAsync();
            accountId = account.Id;
        }

        // 2. Login
        await LoginAsAdmin();

        // 3. Edit Account - Rename and Hide from Dashboard
        var editAccountResponse = await PostForm("/Ledger/EditAccount", new Dictionary<string, string>
        {
            { "Id", accountId.ToString() },
            { "EntityId", entityId.ToString() },
            { "AccountName", "Updated Name" },
            { "AccountType", FinanceAccountType.Liability.ToString() },
            { "Currency", "EUR" },
            { "ShowInDashboard", "false" },
            { "IsArchived", "false" }
        });
        AssertRedirect(editAccountResponse, "/Ledger/Accounts/" + entityId);

        // 4. Verify in DB
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var account = await db.FinanceAccounts.FindAsync(accountId);
            Assert.IsNotNull(account);
            Assert.AreEqual("Updated Name", account.AccountName);
            Assert.AreEqual(FinanceAccountType.Liability, account.AccountType);
            Assert.AreEqual("EUR", account.Currency);
            Assert.IsFalse(account.ShowInDashboard);
        }

        // 5. Verify Dashboard does not show the hidden account
        var dashboardResponse = await Http.GetAsync("/Ledger/Dashboard/" + entityId);
        var dashboardContent = await dashboardResponse.Content.ReadAsStringAsync();
        Assert.IsFalse(dashboardContent.Contains("Updated Name"), "Hidden account should not be in dashboard.");

        // 6. Show it again
        await PostForm("/Ledger/EditAccount", new Dictionary<string, string>
        {
            { "Id", accountId.ToString() },
            { "EntityId", entityId.ToString() },
            { "AccountName", "Updated Name" },
            { "AccountType", FinanceAccountType.Liability.ToString() },
            { "Currency", "EUR" },
            { "ShowInDashboard", "true" },
            { "IsArchived", "false" }
        });

        // 7. Verify Dashboard shows it
        var dashboardResponse2 = await Http.GetAsync("/Ledger/Dashboard/" + entityId);
        var dashboardContent2 = await dashboardResponse2.Content.ReadAsStringAsync();
        Assert.IsTrue(dashboardContent2.Contains("Updated Name"), "Visible account should be in dashboard.");
    }

    private async Task<decimal> GetBalance(EmployeeCenterDbContext db, int accountId)
    {
        var account = await db.FinanceAccounts.FindAsync(accountId);
        var sourceSum = await db.Transactions
            .Where(t => t.SourceAccountId == accountId)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var destinationSum = await db.Transactions
            .Where(t => t.DestinationAccountId == accountId)
            .SumAsync(t => (decimal?)t.Amount * t.ExchangeRate) ?? 0;

        return account!.AccountType switch
        {
            FinanceAccountType.Asset or FinanceAccountType.Expense => destinationSum - sourceSum,
            FinanceAccountType.Liability or FinanceAccountType.Equity or FinanceAccountType.Income => sourceSum - destinationSum,
            _ => 0
        };
    }
}