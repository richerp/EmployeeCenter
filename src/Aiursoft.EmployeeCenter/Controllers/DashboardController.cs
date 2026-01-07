using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.DashboardViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Controllers;

[LimitPerMin]
[Authorize]
public class DashboardController(
    EmployeeCenterDbContext context,
    UserManager<User> userManager) : Controller
{
    [RenderInNavBar(
        NavGroupName = "Features",
        NavGroupOrder = 1,
        CascadedLinksGroupName = "Home",
        CascadedLinksIcon = "home",
        CascadedLinksOrder = 1,
        LinkText = "Index",
        LinkOrder = 1)]
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        var tasks = await context.OnboardingTasks
            .OrderBy(t => t.Order)
            .ToListAsync();

        var logs = await context.OnboardingTaskLogs
            .Where(l => l.UserId == user!.Id)
            .ToListAsync();

        return this.StackView(new IndexViewModel
        {
            Tasks = tasks,
            Logs = logs
        });
    }
}
