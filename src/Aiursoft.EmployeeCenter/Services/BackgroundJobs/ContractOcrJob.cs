using Aiursoft.Canon.BackgroundJobs;
using Aiursoft.EmployeeCenter.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Services.BackgroundJobs;

public class ContractOcrJob(
    EmployeeCenterDbContext db,
    OcrService ocrService,
    ILogger<ContractOcrJob> logger) : IBackgroundJob
{
    public string Name => "Contract OCR Job";
    public string Description => "Scans for contracts that haven't been OCR processed yet and performs OCR recognition.";

    public async Task ExecuteAsync()
    {
        try
        {
            logger.LogInformation("Contract OCR job started");
            
            // Find contracts that don't have OCR results yet
            var unprocessedContractIds = await db.Contracts
                .Where(c => !db.ContractOcrResults.Any(r => r.ContractId == c.Id))
                .OrderByDescending(c => c.CreateTime)
                .Select(c => c.Id)
                .Take(10) // Process 10 at a time to avoid long running job
                .ToListAsync();

            if (unprocessedContractIds.Count == 0)
            {
                logger.LogInformation("No unprocessed contracts found.");
                return;
            }

            logger.LogInformation("Found {Count} unprocessed contracts. Starting OCR processing...", unprocessedContractIds.Count);

            foreach (var contractId in unprocessedContractIds)
            {
                await ocrService.ProcessContractOcrAsync(contractId);
            }

            logger.LogInformation("Contract OCR job completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred in contract OCR job");
        }
    }
}
