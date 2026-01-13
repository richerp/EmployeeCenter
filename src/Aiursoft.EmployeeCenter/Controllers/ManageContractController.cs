using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.ContractViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.EmployeeCenter.Services.FileStorage;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize(Policy = AppPermissionNames.CanViewContractHistory)]
[LimitPerMin]
public class ManageContractController(
    EmployeeCenterDbContext context,
    StorageService storage)
    : Controller
{
    [RenderInNavBar(
        NavGroupName = "Administration",
        NavGroupOrder = 3,
        CascadedLinksGroupName = "HR",
        CascadedLinksIcon = "users",
        CascadedLinksOrder = 1,
        LinkText = "Manage Contracts",
        LinkOrder = 2)]
    public async Task<IActionResult> Index()
    {
        var contracts = await context.Contracts
            .Include(c => c.User)
            .OrderByDescending(c => c.CreateTime)
            .ToListAsync();

        return this.StackView(new ManageViewModel
        {
            Contracts = contracts
        });
    }

    [Authorize(Policy = AppPermissionNames.CanCreateContract)]
    public async Task<IActionResult> Create(string? userId = null)
    {
        return this.StackView(new CreateViewModel
        {
            UserId = userId ?? string.Empty,
            AllUsers = await context.Users.ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanCreateContract)]
    public async Task<IActionResult> Create(CreateViewModel model)
    {
        if (ModelState.IsValid && model.File != null)
        {
            var storePath = Path.Combine(
                "Contracts",
                model.UserId ?? "Shared",
                DateTime.UtcNow.Year.ToString("D4"),
                DateTime.UtcNow.Month.ToString("D2"),
                model.File.FileName);
            
            var relativePath = await storage.Save(storePath, model.File);

            var contract = new Contract
            {
                UserId = model.UserId,
                Name = model.Name,
                FilePath = relativePath,
                Status = model.Status,
                IsPublic = model.IsPublic,
                CreateTime = DateTime.UtcNow
            };
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        model.AllUsers = await context.Users.ToListAsync();
        return this.StackView(model);
    }

    [Authorize(Policy = AppPermissionNames.CanCreateContract)]
    public async Task<IActionResult> Edit(int id)
    {
        var contract = await context.Contracts
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (contract == null) return NotFound();

        return this.StackView(new EditViewModel
        {
            Id = contract.Id,
            UserId = contract.UserId,
            UserName = contract.User?.DisplayName,
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

            if (model.File != null)
            {
                var storePath = Path.Combine(
                    "Contracts",
                    contract.UserId ?? "Shared",
                    DateTime.UtcNow.Year.ToString("D4"),
                    DateTime.UtcNow.Month.ToString("D2"),
                    model.File.FileName);
                var relativePath = await storage.Save(storePath, model.File);
                contract.FilePath = relativePath;
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
