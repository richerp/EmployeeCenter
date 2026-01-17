using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.WeeklyReportViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize]
[LimitPerMin]
public class WeeklyReportController(
    EmployeeCenterDbContext dbContext,
    UserManager<User> userManager,
    IAuthorizationService authorizationService,
    IStringLocalizer<WeeklyReportController> localizer) : Controller
{
    [RenderInNavBar(
        NavGroupName = "Career",
        NavGroupOrder = 1,
        CascadedLinksGroupName = "Development",
        CascadedLinksIcon = "git-branch",
        CascadedLinksOrder = 2,
        LinkText = "Weekly Report",
        LinkOrder = 2)]
    public async Task<IActionResult> Index(string? userId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var canCreate = (await authorizationService.AuthorizeAsync(User, AppPermissionNames.CanCreateWeeklyReport)).Succeeded;
        var canCreateForAnyone = (await authorizationService.AuthorizeAsync(User, AppPermissionNames.CanCreateWeeklyReportForAnyone)).Succeeded;

        var query = dbContext.WeeklyReports
            .Include(r => r.User)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(r => r.UserId == userId);
        }

        var reports = await query
            .OrderByDescending(r => r.WeekStartDate)
            .ThenByDescending(r => r.CreateTime)
            .Take(10)
            .ToListAsync();

        var notepad = await dbContext.Notepads.FirstOrDefaultAsync(n => n.UserId == user.Id);
        
        // Logic for Week Selection and Missing Reports
        // Week starts on Sunday
        var now = DateTime.UtcNow;
        var offset = (int)now.DayOfWeek;
        var thisWeekStart = now.AddDays(-offset).Date;
        
        // Analyze last 50 weeks for the current user to find missing reports
        var cutoffDate = thisWeekStart.AddDays(-49 * 7);
        var userReports = await dbContext.WeeklyReports
            .Where(r => r.UserId == user.Id && (r.WeekStartDate >= cutoffDate || r.CreateTime >= cutoffDate))
            .Select(r => new { r.WeekStartDate, r.CreateTime })
            .ToListAsync();

        var existingWeeks = userReports
            .Select(r => r.WeekStartDate != DateTime.MinValue 
                ? r.WeekStartDate 
                : r.CreateTime.AddDays(-(int)r.CreateTime.DayOfWeek).Date)
            .ToHashSet();

        var availableWeeks = new Dictionary<DateTime, string>();
        for (int i = 0; i < 50; i++)
        {
            var weekStart = thisWeekStart.AddDays(-i * 7);
            if (!existingWeeks.Contains(weekStart))
            {
                var label = $"{weekStart:yyyy-MM-dd} ~ {weekStart.AddDays(6):yyyy-MM-dd}";
                if (weekStart == thisWeekStart) label += " (Current Week)";
                availableWeeks.Add(weekStart, label);
            }
        }

        // Check for missing reports in the last 4 weeks (excluding current week if desired, or including)
        // Requirement: "Historically 4 weeks... if exists a week didn't write, warn."
        // Usually implies past weeks. Let's check previous 4 weeks.
        var hasRecentMissing = false;
        for (int i = 1; i <= 4; i++)
        {
            var pastWeek = thisWeekStart.AddDays(-i * 7);
            if (!existingWeeks.Contains(pastWeek))
            {
                hasRecentMissing = true;
                break;
            }
        }

        var submittedThisWeek = existingWeeks.Contains(thisWeekStart);

        var model = new IndexViewModel
        {
            Reports = reports,
            CanCreate = canCreate,
            CanCreateForAnyone = canCreateForAnyone,
            NotepadContent = notepad?.Content,
            CurrentWeekSubmitted = submittedThisWeek,
            AvailableWeeks = availableWeeks,
            HasRecentMissingReports = hasRecentMissing,
            FilterUserId = userId
        };

        if (canCreateForAnyone)
        {
            model.AllUsers = await dbContext.Users
                .OrderBy(u => u.DisplayName)
                .ToListAsync();
        }

        if (!string.IsNullOrEmpty(userId))
        {
            model.FilterUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        return this.StackView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string content, string? onBehalfOf, DateTime? weekStartDate)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return RedirectToAction(nameof(Index));
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var canCreate = (await authorizationService.AuthorizeAsync(User, AppPermissionNames.CanCreateWeeklyReport)).Succeeded;
        var canCreateForAnyone = (await authorizationService.AuthorizeAsync(User, AppPermissionNames.CanCreateWeeklyReportForAnyone)).Succeeded;

        string targetUserId;
        if (!string.IsNullOrEmpty(onBehalfOf) && canCreateForAnyone)
        {
            targetUserId = onBehalfOf;
        }
        else if (canCreate)
        {
            targetUserId = user.Id;
        }
        else
        {
            return Unauthorized();
        }
        
        // Week validation
        var now = DateTime.UtcNow;
        var offset = (int)now.DayOfWeek;
        var thisWeekStart = now.AddDays(-offset).Date;
        var targetWeek = weekStartDate ?? thisWeekStart;

        // Ensure targetWeek is a Sunday
        if (targetWeek.DayOfWeek != DayOfWeek.Sunday)
        {
             // Fallback or error. Let's align it just in case.
             targetWeek = targetWeek.AddDays(-(int)targetWeek.DayOfWeek).Date;
        }

        // Verify it is within allowed range (last 50 weeks)
        if (targetWeek < thisWeekStart.AddDays(-50 * 7) || targetWeek > thisWeekStart)
        {
            // Invalid date
            return RedirectToAction(nameof(Index));
        }

        // Check if report already exists for this week
        var exists = await dbContext.WeeklyReports
            .AnyAsync(r => r.UserId == targetUserId && 
                           (r.WeekStartDate == targetWeek || 
                           (r.WeekStartDate == DateTime.MinValue && r.CreateTime >= targetWeek && r.CreateTime < targetWeek.AddDays(7))));

        if (exists)
        {
             // Already submitted
             // Maybe show error or just redirect
             return RedirectToAction(nameof(Index));
        }

        var report = new WeeklyReport
        {
            UserId = targetUserId,
            Content = content,
            CreateTime = DateTime.UtcNow,
            WeekStartDate = targetWeek
        };

        dbContext.WeeklyReports.Add(report);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { userId = (string.IsNullOrEmpty(onBehalfOf) || onBehalfOf == user.Id) ? null : onBehalfOf });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var report = await dbContext.WeeklyReports
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null) return NotFound();

        if (report.UserId != user.Id)
        {
            return Unauthorized();
        }

        if (report.CreateTime < DateTime.UtcNow.AddDays(-28))
        {
            return BadRequest(localizer["You can only edit reports published within 4 weeks."]);
        }

        var model = new EditViewModel
        {
            Id = report.Id,
            Content = report.Content,
            WeekStartDate = report.WeekStartDate,
            User = report.User
        };

        return this.StackView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var report = await dbContext.WeeklyReports.FirstOrDefaultAsync(r => r.Id == model.Id);
        if (report == null) return NotFound();

        if (report.UserId != user.Id)
        {
            return Unauthorized();
        }

        if (report.CreateTime < DateTime.UtcNow.AddDays(-28))
        {
            return BadRequest(localizer["You can only edit reports published within 4 weeks."]);
        }

        report.Content = model.Content;
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { userId = user.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNotepad(string? content)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var notepad = await dbContext.Notepads.FirstOrDefaultAsync(n => n.UserId == user.Id);
        if (notepad == null)
        {
            notepad = new Notepad
            {
                UserId = user.Id,
                Content = content ?? string.Empty,
                UpdateTime = DateTime.UtcNow
            };
            dbContext.Notepads.Add(notepad);
        }
        else
        {
            notepad.Content = content ?? string.Empty;
            notepad.UpdateTime = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> LoadMore(DateTime beforeWeek, DateTime beforeCreate, string? userId)
    {
        var query = dbContext.WeeklyReports
            .Include(r => r.User)
            .Where(r => r.WeekStartDate < beforeWeek.ToUniversalTime() || 
                       (r.WeekStartDate == beforeWeek.ToUniversalTime() && r.CreateTime < beforeCreate.ToUniversalTime()))
            .AsNoTracking();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(r => r.UserId == userId);
        }

        var reports = await query
            .OrderByDescending(r => r.WeekStartDate)
            .ThenByDescending(r => r.CreateTime)
            .Take(10)
            .ToListAsync();

        return PartialView("_ReportCards", reports);
    }
}
