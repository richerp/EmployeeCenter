using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Aiursoft.EmployeeCenter.Services;

/// <summary>
/// Service for fetching Chinese public holiday information from external API
/// Uses jiejiariapi.com API with caching to minimize API calls
/// </summary>
public class HolidayService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HolidayService> _logger;
    private const string ApiBaseUrl = "https://timor.tech/api/holiday";

    public HolidayService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<HolidayService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Check if a specific date is a public holiday in China
    /// </summary>
    /// <param name="date">Date to check</param>
    /// <returns>True if the date is a public holiday, false otherwise</returns>
    public async Task<bool> IsPublicHolidayAsync(DateTime date)
    {
        var dateKey = date.ToString("yyyy-MM-dd");
        var cacheKey = $"holiday_{dateKey}";

        if (_cache.TryGetValue<bool>(cacheKey, out var cachedResult))
        {
            return cachedResult;
        }

        try
        {
            // API format: https://timor.tech/api/holiday/year/month-day
            var url = $"{ApiBaseUrl}/{date.Year}/{date.Month:00}-{date.Day:00}";
            var response = await _httpClient.GetStringAsync(url);
            var result = JsonSerializer.Deserialize<HolidayApiResponse>(response);

            var isHoliday = result?.Holiday ?? false;

            // Cache for 24 hours
            _cache.Set(cacheKey, isHoliday, TimeSpan.FromHours(24));

            return isHoliday;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch holiday information for {Date}. Assuming it's not a holiday.", dateKey);
            // On error, cache false for a shorter period (1 hour) to enable retry
            _cache.Set(cacheKey, false, TimeSpan.FromHours(1));
            return false;
        }
    }

    /// <summary>
    /// Get all public holidays in a date range
    /// </summary>
    /// <param name="start">Start date (inclusive)</param>
    /// <param name="end">End date (inclusive)</param>
    /// <returns>HashSet of dates that are public holidays</returns>
    public async Task<HashSet<DateTime>> GetPublicHolidaysInRangeAsync(DateTime start, DateTime end)
    {
        var holidays = new HashSet<DateTime>();
        var currentDate = start.Date;

        while (currentDate <= end.Date)
        {
            if (await IsPublicHolidayAsync(currentDate))
            {
                holidays.Add(currentDate);
            }
            currentDate = currentDate.AddDays(1);
        }

        return holidays;
    }

    /// <summary>
    /// Response model for the holiday API
    /// </summary>
    private class HolidayApiResponse
    {
        public int Code { get; set; }
        public bool? Holiday { get; set; }
    }
}
