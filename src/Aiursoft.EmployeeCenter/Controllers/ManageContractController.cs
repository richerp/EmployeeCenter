using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.ContractViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize(Policy = AppPermissionNames.CanViewContractHistory)]
[LimitPerMin]
public class ManageContractController(
    EmployeeCenterDbContext context)
    : Controller
{
    [RenderInNavBar(
        NavGroupName = "Administration",
        NavGroupOrder = 3,
        CascadedLinksGroupName = "Legal",
        CascadedLinksIcon = "scale",
        CascadedLinksOrder = 5,
        LinkText = "Manage Contracts",
        LinkOrder = 1)]
    public async Task<IActionResult> Index()
    {
        var contracts = await context.Contracts
            .OrderByDescending(c => c.CreateTime)
            .ToListAsync();

        return this.StackView(new ManageViewModel
        {
            Contracts = contracts
        });
    }

    [Authorize(Policy = AppPermissionNames.CanCreateContract)]
    public IActionResult Create()
    {
        return this.StackView(new CreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanCreateContract)]
    public async Task<IActionResult> Create(CreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var contract = new Contract
            {
                Name = model.Name,
                FilePath = model.FilePath!,
                Status = model.Status,
                IsPublic = model.IsPublic,
                CreateTime = DateTime.UtcNow
            };
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return this.StackView(model);
    }

    [Authorize(Policy = AppPermissionNames.CanCreateContract)]
    public async Task<IActionResult> Edit(int id)
    {
        var contract = await context.Contracts
            .FirstOrDefaultAsync(c => c.Id == id);
        if (contract == null) return NotFound();

        return this.StackView(new EditViewModel
        {
            Id = contract.Id,
            Name = contract.Name,
            Status = contract.Status,
            IsPublic = contract.IsPublic
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanCreateContract)]
    public async Task<IActionResult> Edit(EditViewModel model)
    {
        if (ModelState.IsValid)
        {
            var contract = await context.Contracts.FindAsync(model.Id);
            if (contract == null) return NotFound();

            contract.Name = model.Name;
            contract.Status = model.Status;
            contract.IsPublic = model.IsPublic;

            if (!string.IsNullOrWhiteSpace(model.FilePath))
            {
                contract.FilePath = model.FilePath;
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return this.StackView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanCreateContract)]
    public async Task<IActionResult> Delete(int id)
    {
        var contract = await context.Contracts.FindAsync(id);
        if (contract != null)
        {
            context.Contracts.Remove(contract);
            await context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
