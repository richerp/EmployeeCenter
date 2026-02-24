using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.HolidayAdjustmentViewModels;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Aiursoft.EmployeeCenter.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize(Policy = AppPermissionNames.CanManageGlobalSettings)]
[LimitPerMin]
public class HolidayAdjustmentController(EmployeeCenterDbContext context) : Controller
{
    [RenderInNavBar(
        NavGroupName = "Career",
        NavGroupOrder = 1,
        CascadedLinksGroupName = "Leave Management",
        CascadedLinksIcon = "calendar-days",
        CascadedLinksOrder = 5,
        LinkText = "Holiday Adjustments",
        LinkOrder = 4)]
    public async Task<IActionResult> Index()
    {
        var adjustments = await context.AdjustedHolidays
            .OrderByDescending(a => a.Date)
            .ToListAsync();
            
        var model = new IndexViewModel
        {
            Adjustments = adjustments
        };
        
        return this.StackView(model);
    }

    public IActionResult Create()
    {
        return this.StackView(new CreateViewModel
        {
            Date = DateTime.UtcNow.Date,
            Type = HolidayType.WorkDay
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return this.StackView(model);
        }

        var existing = await context.AdjustedHolidays
            .FirstOrDefaultAsync(a => a.Date.Date == model.Date.Date);

        if (existing != null)
        {
            ModelState.AddModelError(nameof(model.Date), "An adjustment for this date already exists.");
            return this.StackView(model);
        }

        var adjustment = new AdjustedHoliday
        {
            Date = model.Date.Date,
            Type = model.Type,
            Reason = model.Reason
        };

        context.AdjustedHolidays.Add(adjustment);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var adjustment = await context.AdjustedHolidays.FindAsync(id);
        if (adjustment != null)
        {
            context.AdjustedHolidays.Remove(adjustment);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
