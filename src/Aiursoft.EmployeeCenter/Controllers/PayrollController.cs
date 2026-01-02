using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.PayrollViewModels;
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
public class PayrollController(
    UserManager<User> userManager,
    EmployeeCenterDbContext context)
    : Controller
{
    [RenderInNavBar(
        NavGroupName = "Personal",
        NavGroupOrder = 1,
        CascadedLinksGroupName = "Payrolls",
        CascadedLinksIcon = "shopping-bag",
        CascadedLinksOrder = 10,
        LinkText = "My Payrolls",
        LinkOrder = 1)]
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var payrolls = await context.Payrolls
            .Where(p => p.OwnerId == user.Id)
            .OrderByDescending(p => p.TargetMonth)
            .ToListAsync();

        return this.StackView(new IndexViewModel
        {
            Payrolls = payrolls
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var payroll = await context.Payrolls
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payroll == null) return NotFound();

        var user = await userManager.GetUserAsync(User);
        if (payroll.OwnerId != user?.Id)
        {
            return Unauthorized();
        }

        return this.StackView(new DetailsViewModel
        {
            Payroll = payroll
        });
    }
}