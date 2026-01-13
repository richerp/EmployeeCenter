using Aiursoft.EmployeeCenter.Authorization;
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
    IAuthorizationService authorizationService,
    EmployeeCenterDbContext context)
    : Controller
{
    [RenderInNavBar(
        NavGroupName = "Personal",
        NavGroupOrder = 2,
        CascadedLinksGroupName = "Contracts",
        CascadedLinksIcon = "file-text",
        CascadedLinksOrder = 2,
        LinkText = "View Contracts",
        LinkOrder = 1)]
    public async Task<IActionResult> Index()
    {
        var canViewHistory = (await authorizationService.AuthorizeAsync(User, AppPermissionNames.CanViewContractHistory)).Succeeded;
        
        var contracts = await context.Contracts
            .Where(c => canViewHistory || c.IsPublic)
            .OrderByDescending(c => c.CreateTime)
            .ToListAsync();

        return this.StackView(new IndexViewModel
        {
            Contracts = contracts
        });
    }
}
