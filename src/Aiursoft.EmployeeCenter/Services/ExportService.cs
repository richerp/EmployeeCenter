using Aiursoft.EmployeeCenter.Configuration;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Services.FileStorage;
using Aiursoft.Scanner.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aiursoft.EmployeeCenter.Services;

public class ExportService(
    EmployeeCenterDbContext db,
    IOptions<AppSettings> appSettings,
    StorageService storageService,
    ILogger<ExportService> logger) : IScopedDependency
{
    private readonly string _exportRoot = appSettings.Value.ExportPath;

    public async Task ExportAsync()
    {
        logger.LogInformation("Starting export task to {ExportRoot}...", _exportRoot);

        // Clear export directory content instead of deleting the directory itself
        // because the directory itself might be a mount point.
        if (Directory.Exists(_exportRoot))
        {
            foreach (var directory in Directory.GetDirectories(_exportRoot))
            {
                Directory.Delete(directory, true);
            }
            foreach (var file in Directory.GetFiles(_exportRoot))
            {
                File.Delete(file);
            }
        }
        else
        {
            Directory.CreateDirectory(_exportRoot);
        }

        await ExportBlueprints();
        await ExportContracts();
        await ExportWeeklyReports();

        logger.LogInformation("Export task completed successfully.");
    }

    private async Task ExportWeeklyReports()
    {
        logger.LogInformation("Exporting weekly reports...");
        var reports = await db.WeeklyReports
            .Include(r => r.User)
            .ToListAsync();

        foreach (var report in reports)
        {
            var weekName = report.WeekStartDate.ToString("yyyy-MM-dd");
            var fullDirectoryPath = Path.Combine(_exportRoot, "WeeklyReports", weekName);
            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }

            var fileName = SanitizeFileName(report.User.DisplayName) + ".md";
            await File.WriteAllTextAsync(Path.Combine(fullDirectoryPath, fileName), report.Content);
        }
    }

    private async Task ExportBlueprints()
    {
        logger.LogInformation("Exporting blueprints...");
        var allFolders = await db.BlueprintFolders.ToListAsync();
        var folderMap = allFolders.ToDictionary(f => f.Id);
        
        var blueprints = await db.Blueprints.ToListAsync();
        foreach (var blueprint in blueprints)
        {
            var pathParts = new List<string>();
            var currentFolderId = blueprint.FolderId;
            while (currentFolderId.HasValue && folderMap.TryGetValue(currentFolderId.Value, out var folder))
            {
                pathParts.Insert(0, SanitizeFileName(folder.Name));
                currentFolderId = folder.ParentFolderId;
            }
            
            var relativePath = Path.Combine(pathParts.ToArray());
            var fullDirectoryPath = Path.Combine(_exportRoot, "Blueprints", relativePath);
            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }
            
            var fileName = SanitizeFileName(blueprint.Title) + ".md";
            await File.WriteAllTextAsync(Path.Combine(fullDirectoryPath, fileName), blueprint.Content);
        }
    }

    private async Task ExportContracts()
    {
        logger.LogInformation("Exporting contracts...");
        var allFolders = await db.ContractFolders.ToListAsync();
        var folderMap = allFolders.ToDictionary(f => f.Id);
        
        var contracts = await db.Contracts.ToListAsync();
        foreach (var contract in contracts)
        {
            var pathParts = new List<string>();
            var currentFolderId = contract.FolderId;
            while (currentFolderId.HasValue && folderMap.TryGetValue(currentFolderId.Value, out var folder))
            {
                pathParts.Insert(0, SanitizeFileName(folder.Name));
                currentFolderId = folder.ParentFolderId;
            }
            
            var relativePath = Path.Combine(pathParts.ToArray());
            var fullDirectoryPath = Path.Combine(_exportRoot, "Contracts", relativePath);
            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }

            var baseFileName = SanitizeFileName(contract.Name);
            
            // 1. Export PDF
            var physicalPath = storageService.GetFilePhysicalPath(contract.FilePath);
            if (File.Exists(physicalPath))
            {
                File.Copy(physicalPath, Path.Combine(fullDirectoryPath, baseFileName + ".pdf"), true);
            }
            else
            {
                logger.LogWarning("Physical file not found for contract {ContractName} at {Path}", contract.Name, physicalPath);
            }

            // 2. Export OCR if available
            var ocrResult = await db.ContractOcrResults.FirstOrDefaultAsync(r => r.ContractId == contract.Id);
            if (ocrResult != null && !string.IsNullOrWhiteSpace(ocrResult.PlainText))
            {
                await File.WriteAllTextAsync(Path.Combine(fullDirectoryPath, baseFileName + ".md"), ocrResult.PlainText);
            }
        }
    }

    private string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(name
            .Select(c => invalidChars.Contains(c) ? '_' : c)
            .ToArray());
    }
}