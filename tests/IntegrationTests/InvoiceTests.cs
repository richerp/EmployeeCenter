using System.Net;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.LedgerViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class InvoiceTests : TestBase
{
    [TestMethod]
    public async Task TestInvoiceFlow()
    {
        // 1. Setup - Create entities
        int sellerId, buyerId;
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var seller = new CompanyEntity
            {
                CompanyName = "Seller Entity",
                CompanyNameEnglish = "Seller Ltd",
                EntityCode = "SE001",
                OfficeAddress = "Seller Address",
                BankName = "Seller Bank",
                BankAccount = "123456",
                BankAccountName = "Seller Ltd"
            };
            var buyer = new CompanyEntity
            {
                CompanyName = "Buyer Entity",
                EntityCode = "BE001",
                OfficeAddress = "Buyer Address"
            };
            db.CompanyEntities.AddRange(seller, buyer);
            await db.SaveChangesAsync();
            sellerId = seller.Id;
            buyerId = buyer.Id;
        }

        // 2. Login as Admin
        await LoginAsAdmin();

        // 3. View Index
        var indexResponse = await Http.GetAsync("/Invoice/Index");
        indexResponse.EnsureSuccessStatusCode();
        var indexContent = await indexResponse.Content.ReadAsStringAsync();
        Assert.IsTrue(indexContent.Contains("Seller Entity"), "Index should contain seller entity name.");
        Assert.IsTrue(indexContent.Contains("Buyer Entity"), "Index should contain buyer entity name.");

        // 4. View Create
        var createResponse = await Http.GetAsync($"/Invoice/Create?sellerId={sellerId}&buyerId={buyerId}");
        createResponse.EnsureSuccessStatusCode();
        var createContent = await createResponse.Content.ReadAsStringAsync();
        Assert.IsTrue(createContent.Contains("Seller Ltd"), "Create page should contain English seller name.");
        Assert.IsTrue(createContent.Contains("Buyer Entity"), "Create page should contain buyer name.");
        Assert.IsTrue(createContent.Contains("Seller Address"), "Create page should contain seller address.");
        Assert.IsTrue(createContent.Contains("Buyer Address"), "Create page should contain buyer address.");

        // 5. Submit Preview
        var previewResponse = await PostForm("/Invoice/Preview", new Dictionary<string, string>
        {
            { "SellerId", sellerId.ToString() },
            { "SellerName", "Seller Ltd" },
            { "SellerAddress", "Seller Address" },
            { "SellerCode", "SE001" },
            { "BuyerId", buyerId.ToString() },
            { "BuyerName", "Buyer Entity" },
            { "BuyerAddress", "Buyer Address" },
            { "InvoiceNo", "INV-TEST-01" },
            { "Date", DateTime.Today.ToString("yyyy-MM-dd") },
            { "Currency", "HKD" },
            { "BankName", "Seller Bank" },
            { "BankAccount", "123456" },
            { "BeneficiaryName", "Seller Ltd" },
            { "Items[0].Description", "Test Item" },
            { "Items[0].Quantity", "1" },
            { "Items[0].UnitPrice", "1000" }
        }, tokenUrl: $"/Invoice/Create?sellerId={sellerId}&buyerId={buyerId}");
        
        previewResponse.EnsureSuccessStatusCode();
        var previewContent = await previewResponse.Content.ReadAsStringAsync();
        Assert.IsTrue(previewContent.Contains("INVOICE"), "Preview should contain the title INVOICE.");
        Assert.IsTrue(previewContent.Contains("INV-TEST-01"), "Preview should contain the invoice number.");
        Assert.IsTrue(previewContent.Contains("Test Item"), "Preview should contain the item description.");
        Assert.IsTrue(previewContent.Contains("1,000.00"), "Preview should contain the formatted unit price.");
    }
}
