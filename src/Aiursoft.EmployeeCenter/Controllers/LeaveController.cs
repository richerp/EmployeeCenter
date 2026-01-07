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
    HolidayService holidayService)
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
            StartDate = DateTime.UtcNow.Date.AddDays(3),
            EndDate = DateTime.UtcNow.Date.AddDays(3)
        };

        return this.StackView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(ApplyViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Validate date range
        var today = DateTime.UtcNow.Date;
        var minDate = today.AddDays(3);
        var maxDate = today.AddDays(30);

        if (model.StartDate < minDate || model.StartDate > maxDate)
        {
            ModelState.AddModelError(nameof(model.StartDate), "Start date must be between 3 and 30 days from today.");
        }

        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date must be on or after start date.");
        }

        if (model.EndDate > maxDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date must be within 30 days from today.");
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
}
