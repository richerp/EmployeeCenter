using Aiursoft.CSTools.Tools;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.Scanner.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.BackgroundJobs;

public class AnnualLeaveAllocationJob(
    ILogger<AnnualLeaveAllocationJob> logger,
    IServiceScopeFactory scopeFactory)
    : IHostedService, IDisposable, ISingletonDependency
{
    private const int IntervalHours = 8;
    private const int StartupDelaySeconds = 25;
    private Timer? _timer;

    public static DateTime LastRunTime = DateTime.MinValue;
    public static bool LastRunSuccess;
    public static int ProcessedUserCount;
    public static int CreatedAllocationCount;
    public static DateTime EstimatedNextRunTime = DateTime.MinValue;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!EntryExtends.IsProgramEntry())
        {
            logger.LogInformation("Skip annual leave allocation job in test environment.");
            return Task.CompletedTask;
        }

        logger.LogInformation(
            "Annual Leave Allocation Background Service is starting. Will run every {Interval} hours.",
            IntervalHours);

        _timer = new Timer(
            DoWork,
            null,
            TimeSpan.FromSeconds(StartupDelaySeconds),
            TimeSpan.FromHours(IntervalHours));

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        try
        {
            logger.LogInformation("Annual leave allocation job started");
            EstimatedNextRunTime = DateTime.UtcNow.AddHours(IntervalHours);

            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EmployeeCenterDbContext>();
            var settings = scope.ServiceProvider.GetRequiredService<GlobalSettingsService>();
            await AllocateAnnualLeave(context, settings);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred in annual leave allocation job");
            LastRunSuccess = false;
        }
    }

    private async Task AllocateAnnualLeave(EmployeeCenterDbContext context, GlobalSettingsService settings)
    {
        var startTime = DateTime.UtcNow;
        LastRunTime = startTime;
        ProcessedUserCount = 0;
        CreatedAllocationCount = 0;

        try
        {
            var currentYear = DateTime.UtcNow.Year;
            var annualLeavePerYear = await settings.GetDecimalSettingAsync(Configuration.SettingsMap.AnnualLeavePerYear);
            logger.LogInformation("Checking annual leave allocations for year {Year}. Annual leave per year: {AnnualLeave}", currentYear, annualLeavePerYear);

            // 获取所有用户
            var allUsers = await context.Users.ToListAsync();
            ProcessedUserCount = allUsers.Count;

            logger.LogInformation("Found {Count} users to process", allUsers.Count);

            foreach (var user in allUsers)
            {
                // Check if allocation exists for current year
                var existingAllocation = await context.LeaveBalances
                    .FirstOrDefaultAsync(lb => lb.UserId == user.Id && lb.Year == currentYear);

                if (existingAllocation == null)
                {
                    // Calculate carry-over from previous year's CURRENT allocation only
                    var previousYear = currentYear - 1;
                    var carriedOver = 0m;

                    var previousAllocation = await context.LeaveBalances
                        .FirstOrDefaultAsync(lb => lb.UserId == user.Id && lb.Year == previousYear);

                    if (previousAllocation != null)
                    {
                        // Calculate how much of previous year's CURRENT allocation was used
                        var previousYearStart = new DateTime(previousYear, 1, 1);
                        var previousYearEnd = previousYearStart.AddYears(1);

                        var usedInPreviousYear = await context.LeaveApplications
                            .Where(la => la.UserId == user.Id
                                && la.LeaveType == LeaveType.AnnualLeave
                                && la.StartDate >= previousYearStart
                                && la.StartDate < previousYearEnd
                                && !la.IsWithdrawn
                                && (la.IsPending || la.IsApproved))
                            .SumAsync(la => la.TotalDays);

                        // Use carried first (FIFO), so deduct from carried first
                        var carriedUsedInPrevious = Math.Min(usedInPreviousYear, previousAllocation.CarriedOverAnnualLeave);
                        var currentUsedInPrevious = usedInPreviousYear - carriedUsedInPrevious;

                        // Unused from previous year's CURRENT allocation can carry over
                        var unusedFromPreviousCurrent = previousAllocation.AnnualLeaveAllocation - currentUsedInPrevious;

                        // Cap at annualLeavePerYear days max
                        carriedOver = Math.Max(0m, Math.Min(annualLeavePerYear, unusedFromPreviousCurrent));

                        // Note: previousAllocation.CarriedOverAnnualLeave EXPIRES (2-year rule)
                    }

                    // Create new allocation with carry-over
                    var newAllocation = new LeaveBalance
                    {
                        UserId = user.Id,
                        Year = currentYear,
                        AnnualLeaveAllocation = annualLeavePerYear,
                        SickLeaveAllocation = LeaveBalanceService.DefaultSickLeavePerYear,
                        CarriedOverAnnualLeave = carriedOver
                    };

                    context.LeaveBalances.Add(newAllocation);
                    CreatedAllocationCount++;

                    logger.LogInformation(
                        "Created leave allocation for user {UserId} ({UserName}) for year {Year}. Carried over: {CarriedOver} days",
                        user.Id, user.UserName, currentYear, carriedOver);
                }
            }

            if (CreatedAllocationCount > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation(
                    "Successfully created {Count} new leave allocations for year {Year}",
                    CreatedAllocationCount, currentYear);
            }
            else
            {
                logger.LogInformation(
                    "All users already have leave allocations for year {Year}. No new allocations created.",
                    currentYear);
            }

            LastRunSuccess = true;
        }
        catch (Exception ex)
        {
            LastRunSuccess = false;
            logger.LogError(ex, "Error during annual leave allocation");
            throw;
        }
        finally
        {
            var duration = DateTime.UtcNow - startTime;
            logger.LogInformation(
                "Annual leave allocation job completed in {Duration}. Processed: {Processed}, Created: {Created}, Success: {Success}",
                duration, ProcessedUserCount, CreatedAllocationCount, LastRunSuccess);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Annual Leave Allocation Background Service is stopping");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
