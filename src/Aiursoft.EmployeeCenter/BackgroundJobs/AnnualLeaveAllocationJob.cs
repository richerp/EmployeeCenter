using Aiursoft.CSTools.Tools;
using Aiursoft.EmployeeCenter.Entities;
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
            await AllocateAnnualLeave(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred in annual leave allocation job");
            LastRunSuccess = false;
        }
    }

    private async Task AllocateAnnualLeave(EmployeeCenterDbContext context)
    {
        var startTime = DateTime.UtcNow;
        LastRunTime = startTime;
        ProcessedUserCount = 0;
        CreatedAllocationCount = 0;

        try
        {
            var currentYear = DateTime.UtcNow.Year;
            logger.LogInformation("Checking annual leave allocations for year {Year}", currentYear);

            // 获取所有用户
            var allUsers = await context.Users.ToListAsync();
            ProcessedUserCount = allUsers.Count;

            logger.LogInformation("Found {Count} users to process", allUsers.Count);

            foreach (var user in allUsers)
            {
                // 检查今年是否已经有配额记录
                var existingAllocation = await context.LeaveBalances
                    .FirstOrDefaultAsync(lb => lb.UserId == user.Id && lb.Year == currentYear);

                if (existingAllocation == null)
                {
                    // 只有当今年没有配额记录时才创建，避免重复发放
                    var newAllocation = new LeaveBalance
                    {
                        UserId = user.Id,
                        Year = currentYear,
                        AnnualLeaveAllocation = 12m,  // 12天年假
                        SickLeaveAllocation = 7m      // 7天病假
                    };

                    context.LeaveBalances.Add(newAllocation);
                    CreatedAllocationCount++;

                    logger.LogInformation(
                        "Created leave allocation for user {UserId} ({UserName}) for year {Year}",
                        user.Id, user.UserName, currentYear);
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
