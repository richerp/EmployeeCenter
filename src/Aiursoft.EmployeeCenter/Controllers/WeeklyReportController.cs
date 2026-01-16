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

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize]
[LimitPerMin]
public class WeeklyReportController(
    EmployeeCenterDbContext dbContext,
    UserManager<User> userManager,
    IAuthorizationService authorizationService) : Controller
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
            .OrderByDescending(r => r.CreateTime)
            .Take(10)
            .ToListAsync();

        var notepad = await dbContext.Notepads.FirstOrDefaultAsync(n => n.UserId == user.Id);
        
        // Check if current week report is submitted (assume week starts on Monday)
        var now = DateTime.UtcNow;
        var offset = (int)now.DayOfWeek - (int)DayOfWeek.Monday;
        if (offset < 0) offset += 7;
        var startOfWeek = now.AddDays(-offset).Date;
        
        var submittedThisWeek = await dbContext.WeeklyReports
            .AnyAsync(r => r.UserId == user.Id && r.CreateTime >= startOfWeek);

        var model = new IndexViewModel
        {
            Reports = reports,
            CanCreate = canCreate,
            CanCreateForAnyone = canCreateForAnyone,
            NotepadContent = notepad?.Content,
            CurrentWeekSubmitted = submittedThisWeek,
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
    public async Task<IActionResult> Create(string content, string? onBehalfOf)
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

        var report = new WeeklyReport
        {
            UserId = targetUserId,
            Content = content,
            CreateTime = DateTime.UtcNow
        };

        dbContext.WeeklyReports.Add(report);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { userId = (string.IsNullOrEmpty(onBehalfOf) || onBehalfOf == user.Id) ? null : onBehalfOf });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNotepad(string content)
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
    public async Task<IActionResult> LoadMore(DateTime before, string? userId)
    {
        var query = dbContext.WeeklyReports
            .Include(r => r.User)
            .Where(r => r.CreateTime < before.ToUniversalTime())
            .AsNoTracking();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(r => r.UserId == userId);
        }

        var reports = await query
            .OrderByDescending(r => r.CreateTime)
            .Take(10)
            .ToListAsync();

        return PartialView("_ReportCards", reports);
    }
}
