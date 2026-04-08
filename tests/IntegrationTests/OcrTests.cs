using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.EmployeeCenter.Services.BackgroundJobs;
using Aiursoft.EmployeeCenter.Services.FileStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.EmployeeCenter.Tests.IntegrationTests;

[TestClass]
public class OcrTests : TestBase
{
    [TestMethod]
    public async Task TestOcrServiceSkipWhenNotConfigured()
    {
        // 1. Setup - Create a scope to resolve scoped services
        using var scope = Server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<StorageService>();
        var ocrService = scope.ServiceProvider.GetRequiredService<OcrService>();
        
        var contract = new Contract
        {
            Name = "Test Contract",
            FilePath = "test-skip.pdf",
            Status = ContractStatus.Active,
            IsPublic = true
        };
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();

        var physicalPath = storage.GetFilePhysicalPath(contract.FilePath);
        var dir = Path.GetDirectoryName(physicalPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        await File.WriteAllTextAsync(physicalPath, "%PDF-1.4 dummy content");

        // 2. Act - Process (should skip because endpoint/token is empty in default test config)
        await ocrService.ProcessContractOcrAsync(contract.Id);
        
        // 3. Assert - No result should be saved
        var result = await ocrService.GetOcrResultByContractIdAsync(contract.Id);
        Assert.IsNull(result); 
    }

    [TestMethod]
    public async Task TestContractOcrJobPicksUpUnprocessed()
    {
        using var scope = Server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
        var job = scope.ServiceProvider.GetRequiredService<ContractOcrJob>();
        
        // Add a contract
        var contract = new Contract
        {
            Name = "Unprocessed Contract",
            FilePath = "unprocessed.pdf",
            Status = ContractStatus.Active,
            IsPublic = true
        };
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();

        // Run the job
        await job.ExecuteAsync();
        
        // Since API is not configured, it should not have a result, 
        // but the job should finish without exception.
        var hasResult = await db.ContractOcrResults.AnyAsync(r => r.ContractId == contract.Id);
        Assert.IsFalse(hasResult);
    }
}
