using Aiursoft.EmployeeCenter.Configuration;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Services.FileStorage;
using Aiursoft.Scanner.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Aiursoft.EmployeeCenter.Services;

public class OcrResponse
{
    public string Status { get; set; } = string.Empty;
    public double DurationS { get; set; }
    public string Device { get; set; } = string.Empty;
    public List<OcrResultItem>? Results { get; set; }
    public string? Error { get; set; }
}

public class OcrResultItem
{
    public List<List<double>> Points { get; set; } = new();
    public string Text { get; set; } = string.Empty;
    public double Confidence { get; set; }
    [JsonProperty("page_num")]
    public int PageNum { get; set; }
}

public class OcrService(
    HttpClient httpClient,
    IOptions<OcrSettings> ocrSettings,
    EmployeeCenterDbContext dbContext,
    StorageService storageService,
    ILogger<OcrService> logger) : ITransientDependency
{
    private readonly OcrSettings _ocrSettings = ocrSettings.Value;

    public async Task ProcessContractOcrAsync(int contractId)
    {
        var contract = await dbContext.Contracts.FindAsync(contractId);
        if (contract == null)
        {
            logger.LogWarning("Contract with ID {ContractId} not found for OCR processing", contractId);
            return;
        }

        if (string.IsNullOrEmpty(_ocrSettings.Endpoint) || string.IsNullOrEmpty(_ocrSettings.BearerToken))
        {
            logger.LogWarning("OCR settings are not configured. Skipping OCR for contract {ContractId}", contractId);
            return;
        }

        contract.OcrAttemptCount++;
        contract.LastOcrAttemptTime = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        try
        {
            var filePath = storageService.GetFilePhysicalPath(contract.FilePath);
            if (!File.Exists(filePath))
            {
                logger.LogError("File not found at {FilePath} for contract {ContractId}", filePath, contractId);
                return;
            }

            using var form = new MultipartFormDataContent();
            using var fileContent = new StreamContent(File.OpenRead(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            var request = new HttpRequestMessage(HttpMethod.Post, _ocrSettings.Endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _ocrSettings.BearerToken);
            request.Content = form;

            var response = await httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var ocrResponse = JsonConvert.DeserializeObject<OcrResponse>(content);
                if (ocrResponse?.Status == "ok")
                {
                    var plainText = ocrResponse.Results != null 
                        ? string.Join("\n", ocrResponse.Results.Select(r => r.Text))
                        : string.Empty;
                        
                    var result = new ContractOcrResult
                    {
                        ContractId = contractId,
                        JsonResult = content,
                        PlainText = plainText
                    };
                    dbContext.ContractOcrResults.Add(result);
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("Successfully processed OCR for contract {ContractId}", contractId);
                }
                else
                {
                    logger.LogError("OCR API returned error status: {Status}, Error: {Error} for contract {ContractId}", 
                        ocrResponse?.Status, ocrResponse?.Error, contractId);
                }
            }
            else
            {
                logger.LogError("OCR API request failed with status {StatusCode}: {Content} for contract {ContractId}", 
                    response.StatusCode, content, contractId);
                
                if (content.Contains("too large", StringComparison.OrdinalIgnoreCase) || content.Contains("50 pages", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogError("PDF is too large for OCR processing: contract {ContractId}", contractId);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing OCR for contract {ContractId}", contractId);
        }
    }

    public async Task<string?> GetOcrResultByContractIdAsync(int contractId)
    {
        var result = await dbContext.ContractOcrResults
            .Where(r => r.ContractId == contractId)
            .OrderByDescending(r => r.CreateTime)
            .FirstOrDefaultAsync();

        return result?.JsonResult;
    }
}
