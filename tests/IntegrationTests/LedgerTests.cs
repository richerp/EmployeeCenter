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

        // 8. Edit Transaction
        int transactionId;
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            transactionId = db.Transactions.First(t => t.Description == "Sleek fee").Id;
        }

        var editTransactionResponse = await PostForm("/Ledger/EditTransaction", new Dictionary<string, string>
        {
            { "EntityId", entityId.ToString() },
            { "TransactionId", transactionId.ToString() },
            { "Description", "Sleek fee updated" },
            { "SourceAccountId", bankId.ToString() },
            { "DestinationAccountId", expenseAccountId.ToString() },
            { "Amount", "4000" },
            { "ExchangeRate", "1" },
            { "TransactionTime", DateTime.UtcNow.AddDays(-1).ToString("O") }
        });
        AssertRedirect(editTransactionResponse, "/Ledger/Dashboard/" + entityId);

        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var transaction = await db.Transactions.FindAsync(transactionId);
            Assert.AreEqual("Sleek fee updated", transaction!.Description);
            Assert.AreEqual(4000, transaction.Amount);
            
            // Bank: 10000 - 4000 = 6000
            Assert.AreEqual(6000, await GetBalance(db, bankId));
        }

        // 9. Delete Transaction
        var deleteTransactionResponse = await PostForm("/Ledger/DeleteTransaction", new Dictionary<string, string>
        {
            { "id", transactionId.ToString() },
            { "entityId", entityId.ToString() }
        });
        AssertRedirect(deleteTransactionResponse, "/Ledger/Dashboard/" + entityId);

        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var transaction = await db.Transactions.FindAsync(transactionId);
            Assert.IsNull(transaction);
            
            // Bank: 10000 - 0 = 10000 (since only one transaction remains)
            Assert.AreEqual(10000, await GetBalance(db, bankId));
        }
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