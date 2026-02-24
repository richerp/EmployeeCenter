using Aiursoft.EmployeeCenter.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace Aiursoft.EmployeeCenter.Services;

/// <summary>
/// Service for fetching Chinese public holiday information from external API
/// Uses api.haoshenqi.top API with caching to minimize API calls, and honors local overrides.
/// </summary>
public class HolidayService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HolidayService> _logger;
    private readonly EmployeeCenterDbContext _context;
    private const string ApiBaseUrl = "http://api.haoshenqi.top/holiday";

    public HolidayService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<HolidayService> logger,
        EmployeeCenterDbContext context)
    {
        _httpClient = httpClientFactory.CreateClient();
        _cache = cache;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Check if a specific date is a public holiday in China, respecting local adjustments.
    /// </summary>
    /// <param name="date">Date to check</param>
    /// <returns>True if the date is a public holiday, false otherwise</returns>
    public async Task<bool> IsPublicHolidayAsync(DateTime date)
    {
        var targetDate = date.Date;

        // 1. Check local manual adjustments first (overrides API logic)
        var adjustment = await _context.AdjustedHolidays
            .FirstOrDefaultAsync(a => a.Date.Date == targetDate);

        if (adjustment != null)
        {
            return adjustment.Type == HolidayType.RestDay;
        }

        // 2. Fallback to API logic
        var dateKey = date.ToString("yyyy-MM-dd");
        var cacheKey = $"holiday_{dateKey}";

        if (_cache.TryGetValue<bool>(cacheKey, out var cachedResult))
        {
            return cachedResult;
        }

        try
        {
            // API format: http://api.haoshenqi.top/holiday?date=yyyy-MM-dd
            var url = $"{ApiBaseUrl}?date={dateKey}";
            var response = await _httpClient.GetStringAsync(url);
            var results = JsonSerializer.Deserialize<List<HolidayApiResponse>>(response);

            // Status: 0 = working day, 1 = weekend, 2 = statutory holiday, 3 = major statutory holiday
            var status = results?.FirstOrDefault()?.Status ?? 0;
            var isHoliday = status == 1 || status == 2 || status == 3;

            // Cache for 7 days (holidays don't change retroactively)
            _cache.Set(cacheKey, isHoliday, TimeSpan.FromDays(7));

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
        // [{"date":"2026-01-01","year":2026,"month":1,"day":1,"status":3}]



        /// <summary>
        /// Status: 0 = working day, 1 = weekend, 2 = statutory holiday, 3 = major statutory holiday
        /// </summary>
        [JsonPropertyName("status")]
        public int Status { get; set; }
    }
}
