using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.ContractViewModels;
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
public class ContractController(
    UserManager<User> userManager,
    EmployeeCenterDbContext context)
    : Controller
{
    [RenderInNavBar(
        NavGroupName = "Personal",
        NavGroupOrder = 2,
        CascadedLinksGroupName = "Contracts",
        CascadedLinksIcon = "file-text",
        CascadedLinksOrder = 2,
        LinkText = "My Contracts",
        LinkOrder = 1)]
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var contracts = await context.Contracts
            .Include(c => c.User)
            .Where(c => c.UserId == user.Id || c.IsPublic)
            .OrderByDescending(c => c.CreateTime)
            .ToListAsync();

        return this.StackView(new IndexViewModel
        {
            Contracts = contracts
        });
    }
}
