using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.LeaveViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize]
[LimitPerMin]
public class LeaveController(
    UserManager<User> userManager,
    EmployeeCenterDbContext context,
    LeaveBalanceService leaveBalanceService,
    HolidayService holidayService,
    IAuthorizationService authorizationService)
    : Controller
{
    [RenderInNavBar(
        NavGroupName = "Features",
        NavGroupOrder = 2,
        CascadedLinksGroupName = "Leave Management",
        CascadedLinksIcon = "calendar-days",
        CascadedLinksOrder = 20,
        LinkText = "My Leave Balance",
        LinkOrder = 1)]
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var currentYear = DateTime.UtcNow.Year;

        // Ensure leave allocation exists
        await leaveBalanceService.EnsureLeaveAllocationExistsAsync(user.Id, currentYear);

        // Get leave balances
        var allocation = await context.LeaveBalances
            .FirstAsync(lb => lb.UserId == user.Id && lb.Year == currentYear);

        var carriedOver = await leaveBalanceService.GetCarriedOverAnnualLeaveAsync(user.Id, currentYear);
        var remainingAnnual = await leaveBalanceService.GetRemainingAnnualLeaveAsync(user.Id, currentYear);
        var remainingSick = await leaveBalanceService.GetRemainingSickLeaveAsync(user.Id, currentYear);

        // Get leave history
        var leaveHistory = await context.LeaveApplications
            .Where(la => la.UserId == user.Id)
            .OrderByDescending(la => la.SubmittedAt)
            .ToListAsync();

        var model = new IndexViewModel
        {
            AnnualLeaveAllocation = allocation.AnnualLeaveAllocation,
            CarriedOverAnnualLeave = carriedOver,
            RemainingAnnualLeave = remainingAnnual,
            SickLeaveAllocation = allocation.SickLeaveAllocation,
            RemainingSickLeave = remainingSick,
            LeaveHistory = leaveHistory,
            CanApplyLeave = remainingAnnual > 0 || remainingSick > 0,
            CurrentYear = currentYear
        };

        return this.StackView(model);
    }

    public async Task<IActionResult> Apply()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var currentYear = DateTime.UtcNow.Year;
        var remainingAnnual = await leaveBalanceService.GetRemainingAnnualLeaveAsync(user.Id, currentYear);
        var remainingSick = await leaveBalanceService.GetRemainingSickLeaveAsync(user.Id, currentYear);

        if (remainingAnnual <= 0 && remainingSick <= 0)
        {
            return RedirectToAction(nameof(Index));
        }

        var model = new ApplyViewModel
        {
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date
        };

        return this.StackView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(ApplyViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Validate date range - allow from 7 days ago to end of current year
        var today = DateTime.UtcNow.Date;
        var minDate = today.AddDays(-7); // Allow up to 7 days in the past for retroactive entry
        var maxDate = new DateTime(today.Year, 12, 31); // End of current year

        if (model.StartDate < minDate)
        {
            ModelState.AddModelError(nameof(model.StartDate), "Start date cannot be more than 7 days in the past.");
        }

        if (model.StartDate > maxDate)
        {
            ModelState.AddModelError(nameof(model.StartDate), "Start date must be within the current year.");
        }

        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date must be on or after start date.");
        }

        if (model.EndDate > maxDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date must be within the current year.");
        }

        // Calculate working days
        var holidays = await holidayService.GetPublicHolidaysInRangeAsync(model.StartDate, model.EndDate);
        var workingDays = await leaveBalanceService.CalculateWorkingDaysAsync(model.StartDate, model.EndDate, holidays);

        if (workingDays <= 0)
        {
            ModelState.AddModelError("", "The selected date range contains no working days.");
        }

        model.TotalDays = workingDays;

        // Check if user has enough leave balance
        var currentYear = DateTime.UtcNow.Year;
        if (model.LeaveType == LeaveType.AnnualLeave)
        {
            var remainingAnnual = await leaveBalanceService.GetRemainingAnnualLeaveAsync(user.Id, currentYear);
            if (workingDays > remainingAnnual)
            {
                ModelState.AddModelError("", $"Not enough annual leave balance. You have {remainingAnnual} days remaining.");
            }
        }
        else if (model.LeaveType == LeaveType.SickLeave)
        {
            var remainingSick = await leaveBalanceService.GetRemainingSickLeaveAsync(user.Id, currentYear);
            if (workingDays > remainingSick)
            {
                ModelState.AddModelError("", $"Not enough sick leave balance. You have {remainingSick} days remaining.");
            }
        }

        if (!ModelState.IsValid)
        {
            return this.StackView(model);
        }

        // Create leave application
        var leaveApplication = new LeaveApplication
        {
            UserId = user.Id,
            LeaveType = model.LeaveType,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            TotalDays = workingDays,
            Reason = model.Reason,
            IsPending = true,
            IsApproved = false
        };

        context.LeaveApplications.Add(leaveApplication);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// AJAX endpoint to get holidays in a date range for calendar rendering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHolidays(DateTime start, DateTime end)
    {
        var holidays = await holidayService.GetPublicHolidaysInRangeAsync(start, end);
        var holidayStrings = holidays.Select(h => h.ToString("yyyy-MM-dd")).ToList();
        return Json(new { holidays = holidayStrings });
    }
    /// <summary>
    /// Withdraw a leave application
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var leave = await context.LeaveApplications
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == user.Id);

        if (leave == null)
        {
            return NotFound();
        }

        if (leave.IsWithdrawn)
        {
            return RedirectToAction(nameof(Index));
        }

        // Cannot withdraw if the leave has already started (server time is UTC as per requirement)
        if (DateTime.UtcNow.Date >= leave.StartDate.Date)
        {
            return BadRequest("Cannot withdraw leave that has already started.");
        }

        leave.IsWithdrawn = true;
        leave.WithdrawnAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Incoming()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var canApproveAny = (await authorizationService.AuthorizeAsync(User, AppPermissionNames.CanApproveAnyLeave)).Succeeded;

        // Get leaves that are pending approval
        // Condition:
        // 1. Pending (IsPending = true)
        // 2. Not Withdrawn (IsWithdrawn = false)
        // 3. User is Manager OR User has CanApproveAnyLeave
        // 4. Exclude own leaves (optional, but usually you don't approve your own leave if you are your own manager? Logic says direct manager approves. If I am my own manager, maybe? But usually safe to exclude self if we want strict control. Requirement didn't say. Let's include all valid ones.)
        // Actually, for "User is Manager", we need to check if the applicant's ManagerId is the current user.

        var query = context.LeaveApplications
            .Include(l => l.User)
            .Where(l => l.IsPending && !l.IsWithdrawn);

        if (!canApproveAny)
        {
            query = query.Where(l => l.User!.ManagerId == user.Id);
        }

        var incomingLeaves = await query
            .OrderBy(l => l.SubmittedAt)
            .ToListAsync();

        return this.StackView(new IncomingViewModel
        {
            IncomingLeaves = incomingLeaves
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, bool approved)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var leave = await context.LeaveApplications
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (leave == null) return NotFound();

        // Check permission
        // 1. Direct Manager
        // 2. CanApproveAnyLeave
        var canApproveAny = (await authorizationService.AuthorizeAsync(User, AppPermissionNames.CanApproveAnyLeave)).Succeeded;
        var isManager = leave.User!.ManagerId == user.Id;

        if (!canApproveAny && !isManager)
        {
            return Unauthorized();
        }

        if (!leave.IsPending || leave.IsWithdrawn)
        {
            return BadRequest("Leave application is not pending or has been withdrawn.");
        }

        leave.IsPending = false;
        leave.IsApproved = approved;
        leave.ReviewedAt = DateTime.UtcNow;
        leave.ReviewedById = user.Id;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Incoming));
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var history = await context.LeaveApplications
            .Include(l => l.User)
            .Where(l => l.ReviewedById == user.Id)
            .OrderByDescending(l => l.ReviewedAt)
            .ToListAsync();

        return this.StackView(new HistoryViewModel
        {
            ApprovedLeaves = history
        });
    }
}
