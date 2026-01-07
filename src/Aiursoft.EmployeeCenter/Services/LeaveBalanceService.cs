using Aiursoft.EmployeeCenter.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Services;

/// <summary>
/// Service for calculating leave balances and managing leave allocations
/// </summary>
public class LeaveBalanceService
{
    private readonly EmployeeCenterDbContext _context;
    private readonly HolidayService _holidayService;

    // Company policies
    private const decimal AnnualLeavePerYear = 12m;
    private const decimal SickLeavePerYear = 7m;

    public LeaveBalanceService(
        EmployeeCenterDbContext context,
        HolidayService holidayService)
    {
        _context = context;
        _holidayService = holidayService;
    }

    /// <summary>
    /// Ensure leave allocation record exists for a user in a specific year
    /// Creates with default values if not exists
    /// </summary>
    public async Task EnsureLeaveAllocationExistsAsync(string userId, int year)
    {
        var existingAllocation = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.UserId == userId && lb.Year == year);

        if (existingAllocation == null)
        {
            var newAllocation = new LeaveBalance
            {
                UserId = userId,
                Year = year,
                AnnualLeaveAllocation = AnnualLeavePerYear,
                SickLeaveAllocation = SickLeavePerYear
            };

            _context.LeaveBalances.Add(newAllocation);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Calculate carried over annual leave from the previous year
    /// Annual leave can carry over to the next year but not to the third year
    /// </summary>
    public async Task<decimal> GetCarriedOverAnnualLeaveAsync(string userId, int currentYear)
    {
        var previousYear = currentYear - 1;

        // Get previous year's allocation
        var previousAllocation = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.UserId == userId && lb.Year == previousYear);

        if (previousAllocation == null)
        {
            return 0m;
        }

        var startOfPreviousYear = new DateTime(previousYear, 1, 1);
        var endOfPreviousYear = startOfPreviousYear.AddYears(1);

        // Calculate used annual leave in previous year
        var usedInPreviousYear = await _context.LeaveApplications
            .Where(la => la.UserId == userId
                && la.LeaveType == LeaveType.AnnualLeave
                && la.StartDate >= startOfPreviousYear
                && la.StartDate < endOfPreviousYear
                && !la.IsWithdrawn // Exclude withdrawn
                && !la.IsPending) // Only count approved/rejected that were actually processed
            .Where(la => la.IsApproved) // Only count approved ones
            .SumAsync(la => la.TotalDays);

        var remainingFromPreviousYear = previousAllocation.AnnualLeaveAllocation - usedInPreviousYear;

        // Only positive balances carry over, max is the full allocation
        return Math.Max(0, Math.Min(remainingFromPreviousYear, AnnualLeavePerYear));
    }

    /// <summary>
    /// Get remaining annual leave for a user in a specific year
    /// </summary>
    public async Task<decimal> GetRemainingAnnualLeaveAsync(string userId, int year)
    {
        await EnsureLeaveAllocationExistsAsync(userId, year);

        var allocation = await _context.LeaveBalances
            .FirstAsync(lb => lb.UserId == userId && lb.Year == year);

        var carriedOver = await GetCarriedOverAnnualLeaveAsync(userId, year);

        var startOfYear = new DateTime(year, 1, 1);
        var endOfYear = startOfYear.AddYears(1);

        // Calculate used annual leave (including pending applications to prevent duplicate requests)
        var usedThisYear = await _context.LeaveApplications
            .Where(la => la.UserId == userId
                && la.LeaveType == LeaveType.AnnualLeave
                && la.StartDate >= startOfYear
                && la.StartDate < endOfYear
                && !la.IsWithdrawn // Exclude withdrawn
                && (la.IsPending || la.IsApproved)) // Exclude rejected
            .SumAsync(la => la.TotalDays);

        var totalAvailable = allocation.AnnualLeaveAllocation + carriedOver;
        var remaining = totalAvailable - usedThisYear;

        return Math.Max(0, remaining);
    }

    /// <summary>
    /// Get remaining sick leave for a user in a specific year
    /// Sick leave does not carry over
    /// </summary>
    public async Task<decimal> GetRemainingSickLeaveAsync(string userId, int year)
    {
        await EnsureLeaveAllocationExistsAsync(userId, year);

        var allocation = await _context.LeaveBalances
            .FirstAsync(lb => lb.UserId == userId && lb.Year == year);

        var startOfYear = new DateTime(year, 1, 1);
        var endOfYear = startOfYear.AddYears(1);

        // Calculate used sick leave (including pending applications)
        var usedThisYear = await _context.LeaveApplications
            .Where(la => la.UserId == userId
                && la.LeaveType == LeaveType.SickLeave
                && la.StartDate >= startOfYear
                && la.StartDate < endOfYear
                && !la.IsWithdrawn // Exclude withdrawn
                && (la.IsPending || la.IsApproved)) // Exclude rejected
            .SumAsync(la => la.TotalDays);

        var remaining = allocation.SickLeaveAllocation - usedThisYear;

        return Math.Max(0, remaining);
    }

    /// <summary>
    /// Calculate working days in a date range, excluding weekends and public holidays
    /// </summary>
    public async Task<decimal> CalculateWorkingDaysAsync(DateTime start, DateTime end, HashSet<DateTime>? publicHolidays = null)
    {
        // Fetch public holidays if not provided
        publicHolidays ??= await _holidayService.GetPublicHolidaysInRangeAsync(start, end);

        var workingDays = 0m;
        var currentDate = start.Date;

        while (currentDate <= end.Date)
        {
            // Skip weekends (Saturday = 6, Sunday = 0)
            var dayOfWeek = (int)currentDate.DayOfWeek;
            var isWeekend = dayOfWeek == 0 || dayOfWeek == 6;

            // Skip public holidays
            var isPublicHoliday = publicHolidays.Contains(currentDate);

            if (!isWeekend && !isPublicHoliday)
            {
                workingDays += 1m;
            }

            currentDate = currentDate.AddDays(1);
        }

        return workingDays;
    }
}
