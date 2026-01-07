using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize]
[LimitPerMin]
public class OnboardingController(
    EmployeeCenterDbContext context,
    UserManager<User> userManager) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartTask(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var task = await context.OnboardingTasks.FindAsync(id);
        if (task == null) return NotFound();

        var log = await context.OnboardingTaskLogs
            .FirstOrDefaultAsync(l => l.UserId == user.Id && l.TaskId == id && l.CompletionTime == null);

        if (log == null)
        {
            log = new OnboardingTaskLog
            {
                UserId = user.Id,
                TaskId = id,
                StartTime = DateTime.UtcNow
            };
            context.OnboardingTaskLogs.Add(log);
            await context.SaveChangesAsync();
        }

        return Redirect(task.StartLink);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteTask(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var log = await context.OnboardingTaskLogs
            .FirstOrDefaultAsync(l => l.UserId == user.Id && l.TaskId == id && l.CompletionTime == null);

        if (log == null) return BadRequest("Task not started or already completed.");

        if (DateTime.UtcNow < log.StartTime.AddSeconds(30))
        {
            return BadRequest("You must wait at least 30 seconds after starting the task before completing it.");
        }

        log.CompletionTime = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return RedirectToAction("Index", "Dashboard");
    }
}
