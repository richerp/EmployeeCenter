using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.CompanyEntityViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize]
[LimitPerMin]
public class CompanyEntityController(
    EmployeeCenterDbContext dbContext,
    UserManager<User> userManager) : Controller
{
    [HttpGet]
    [RenderInNavBar(
        NavGroupName = "Administration",
        NavGroupOrder = 5,
        CascadedLinksGroupName = "Info",
        CascadedLinksIcon = "briefcase",
        CascadedLinksOrder = 1,
        LinkText = "Company Entities",
        LinkOrder = 1)]
    public async Task<IActionResult> Index()
    {
        var entities = await dbContext.CompanyEntities
            .OrderByDescending(t => t.CreationTime)
            .ToListAsync();
        var model = new IndexViewModel
        {
            Entities = entities
        };
        return this.StackView(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var entity = await dbContext.CompanyEntities.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        var model = new DetailsViewModel
        {
            Entity = entity
        };
        return this.StackView(model);
    }

    [HttpGet]
    [Authorize(Policy = AppPermissionNames.CanManageCompanyEntities)]
    public IActionResult Create()
    {
        return this.StackView(new CreateViewModel
        {
            CompanyName = string.Empty,
            EntityCode = string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanManageCompanyEntities)]
    public async Task<IActionResult> Create(CreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return this.StackView(model);
        }

        var entity = new CompanyEntity
        {
            CompanyName = model.CompanyName,
            CompanyNameEnglish = model.CompanyNameEnglish,
            EntityCode = model.EntityCode,
            CINumber = model.CINumber,
            RegisteredAddress = model.RegisteredAddress,
            OfficeAddress = model.OfficeAddress,
            LegalRepresentative = model.LegalRepresentative,
            CompanyType = model.CompanyType,
            EstablishmentDate = model.EstablishmentDate,
            ExpiryDate = model.ExpiryDate,
            BankName = model.BankName,
            BankAccount = model.BankAccount,
            BankAccountName = model.BankAccountName,
            LogoPath = model.LogoPath,
            SealPath = model.SealPath,
            LicensePath = model.LicensePath,
            RegisteredCapital = model.RegisteredCapital,
            OperationStatus = model.OperationStatus,
            SCRLocation = model.SCRLocation,
            CompanySecretary = model.CompanySecretary
        };

        dbContext.CompanyEntities.Add(entity);
        await dbContext.SaveChangesAsync();

        var user = await userManager.GetUserAsync(User);
        var log = new CompanyEntityLog
        {
            CompanyEntityId = entity.Id,
            UserId = user!.Id,
            Action = "Create",
            Details = JsonConvert.SerializeObject(model),
            Snapshot = JsonConvert.SerializeObject(entity)
        };
        dbContext.CompanyEntityLogs.Add(log);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = AppPermissionNames.CanManageCompanyEntities)]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await dbContext.CompanyEntities.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        var model = new EditViewModel
        {
            Id = entity.Id,
            CompanyName = entity.CompanyName,
            CompanyNameEnglish = entity.CompanyNameEnglish,
            EntityCode = entity.EntityCode,
            CINumber = entity.CINumber,
            RegisteredAddress = entity.RegisteredAddress,
            OfficeAddress = entity.OfficeAddress,
            LegalRepresentative = entity.LegalRepresentative,
            CompanyType = entity.CompanyType,
            EstablishmentDate = entity.EstablishmentDate,
            ExpiryDate = entity.ExpiryDate,
            BankName = entity.BankName,
            BankAccount = entity.BankAccount,
            BankAccountName = entity.BankAccountName,
            LogoPath = entity.LogoPath,
            SealPath = entity.SealPath,
            LicensePath = entity.LicensePath,
            RegisteredCapital = entity.RegisteredCapital,
            OperationStatus = entity.OperationStatus,
            SCRLocation = entity.SCRLocation,
            CompanySecretary = entity.CompanySecretary
        };

        return this.StackView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanManageCompanyEntities)]
    public async Task<IActionResult> Edit(EditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return this.StackView(model);
        }

        var entity = await dbContext.CompanyEntities.FindAsync(model.Id);
        if (entity == null)
        {
            return NotFound();
        }

        var oldSnapshot = JsonConvert.SerializeObject(entity);

        entity.CompanyName = model.CompanyName;
        entity.CompanyNameEnglish = model.CompanyNameEnglish;
        entity.EntityCode = model.EntityCode;
        entity.CINumber = model.CINumber;
        entity.RegisteredAddress = model.RegisteredAddress;
        entity.OfficeAddress = model.OfficeAddress;
        entity.LegalRepresentative = model.LegalRepresentative;
        entity.CompanyType = model.CompanyType;
        entity.EstablishmentDate = model.EstablishmentDate;
        entity.ExpiryDate = model.ExpiryDate;
        entity.BankName = model.BankName;
        entity.BankAccount = model.BankAccount;
        entity.BankAccountName = model.BankAccountName;
        entity.LogoPath = model.LogoPath;
        entity.SealPath = model.SealPath;
        entity.LicensePath = model.LicensePath;
        entity.RegisteredCapital = model.RegisteredCapital;
        entity.OperationStatus = model.OperationStatus;
        entity.SCRLocation = model.SCRLocation;
        entity.CompanySecretary = model.CompanySecretary;
        entity.UpdateTime = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        var user = await userManager.GetUserAsync(User);
        var log = new CompanyEntityLog
        {
            CompanyEntityId = entity.Id,
            UserId = user!.Id,
            Action = "Update",
            Details = $"From: {oldSnapshot} To: {JsonConvert.SerializeObject(entity)}",
            Snapshot = JsonConvert.SerializeObject(entity)
        };
        dbContext.CompanyEntityLogs.Add(log);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanManageCompanyEntities)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await dbContext.CompanyEntities.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        var user = await userManager.GetUserAsync(User);
        var log = new CompanyEntityLog
        {
            CompanyEntityId = entity.Id,
            UserId = user!.Id,
            Action = "Delete",
            Details = "Deleted",
            Snapshot = JsonConvert.SerializeObject(entity)
        };
        dbContext.CompanyEntityLogs.Add(log);

        dbContext.CompanyEntities.Remove(entity);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
