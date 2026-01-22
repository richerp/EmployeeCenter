using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.ServicesViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize]
[LimitPerMin]
public class ServicesController(
    EmployeeCenterDbContext context)
    : Controller
{
    [RenderInNavBar(
        NavGroupName = "Career",
        NavGroupOrder = 1,
        CascadedLinksGroupName = "Development",
        CascadedLinksIcon = "git-branch",
        CascadedLinksOrder = 2,
        LinkText = "Services",
        LinkOrder = 2)]
    public async Task<IActionResult> Index()
    {
        var services = await context.Services
            .Include(s => s.Owner)
            .Include(s => s.CrossEntityLink)
            .Include(s => s.Location)
            .Include(s => s.DnsProvider)
            .OrderBy(s => s.Domain)
            .ToListAsync();

        return this.StackView(new IndexViewModel
        {
            Services = services
        });
    }

    [Authorize(Policy = AppPermissionNames.CanManageServices)]
    public async Task<IActionResult> Create()
    {
        return this.StackView(new CreateServiceViewModel
        {
            AllOwners = await context.CompanyEntities.ToListAsync(),
            AllLocations = await context.Locations.ToListAsync(),
            AllDnsProviders = await context.DnsProviders.ToListAsync(),
            AllServices = await context.Services.ToListAsync(),
            AllAssets = await context.Assets.ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanManageServices)]
    public async Task<IActionResult> Create(CreateServiceViewModel model)
    {
        if (ModelState.IsValid)
        {
            var service = new Service
            {
                Domain = model.Domain,
                OwnerId = model.OwnerId,
                CrossEntityLinkId = model.CrossEntityLinkId,
                Protocols = model.Protocols,
                LocationId = model.LocationId,
                ServerIp = model.ServerIp,
                ServerId = model.ServerId,
                DnsProviderId = model.DnsProviderId,
                IsViaFrps = model.IsViaFrps,
                IsCloudflareProxied = model.IsCloudflareProxied,
                IsOnline = model.IsOnline,
                IsSelfDeveloped = model.IsSelfDeveloped,
                Remark = model.Remark,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Services.Add(service);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        model.AllOwners = await context.CompanyEntities.ToListAsync();
        model.AllLocations = await context.Locations.ToListAsync();
        model.AllDnsProviders = await context.DnsProviders.ToListAsync();
        model.AllServices = await context.Services.ToListAsync();
        model.AllAssets = await context.Assets.ToListAsync();
        return this.StackView(model);
    }

    [Authorize(Policy = AppPermissionNames.CanManageServices)]
    public async Task<IActionResult> Edit(int id)
    {
        var service = await context.Services.FindAsync(id);
        if (service == null) return NotFound();

        return this.StackView(new EditServiceViewModel
        {
            Id = service.Id,
            Domain = service.Domain,
            OwnerId = service.OwnerId,
            CrossEntityLinkId = service.CrossEntityLinkId,
            Protocols = service.Protocols,
            LocationId = service.LocationId,
            ServerIp = service.ServerIp,
            ServerId = service.ServerId,
            DnsProviderId = service.DnsProviderId,
            IsViaFrps = service.IsViaFrps,
            IsCloudflareProxied = service.IsCloudflareProxied,
            IsOnline = service.IsOnline,
            IsSelfDeveloped = service.IsSelfDeveloped,
            Remark = service.Remark,
            AllOwners = await context.CompanyEntities.ToListAsync(),
            AllLocations = await context.Locations.ToListAsync(),
            AllDnsProviders = await context.DnsProviders.ToListAsync(),
            AllServices = await context.Services.Where(s => s.Id != id).ToListAsync(),
            AllAssets = await context.Assets.ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanManageServices)]
    public async Task<IActionResult> Edit(EditServiceViewModel model)
    {
        var service = await context.Services.FindAsync(model.Id);
        if (service == null) return NotFound();

        if (ModelState.IsValid)
        {
            service.Domain = model.Domain;
            service.OwnerId = model.OwnerId;
            service.CrossEntityLinkId = model.CrossEntityLinkId;
            service.Protocols = model.Protocols;
            service.LocationId = model.LocationId;
            service.ServerIp = model.ServerIp;
            service.ServerId = model.ServerId;
            service.DnsProviderId = model.DnsProviderId;
            service.IsViaFrps = model.IsViaFrps;
            service.IsCloudflareProxied = model.IsCloudflareProxied;
            service.IsOnline = model.IsOnline;
            service.IsSelfDeveloped = model.IsSelfDeveloped;
            service.Remark = model.Remark;
            service.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        model.AllOwners = await context.CompanyEntities.ToListAsync();
        model.AllLocations = await context.Locations.ToListAsync();
        model.AllDnsProviders = await context.DnsProviders.ToListAsync();
        model.AllServices = await context.Services.Where(s => s.Id != model.Id).ToListAsync();
        model.AllAssets = await context.Assets.ToListAsync();
        return this.StackView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanManageServices)]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await context.Services.FindAsync(id);
        if (service == null) return NotFound();

        // Check if any service links to this one
        if (await context.Services.AnyAsync(s => s.CrossEntityLinkId == id))
        {
            return BadRequest("Cannot delete a service that is linked by another service.");
        }

        context.Services.Remove(service);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = AppPermissionNames.CanManageServices)]
    public async Task<IActionResult> DnsProviders()
    {
        return this.StackView(new ManageDnsProvidersViewModel
        {
            DnsProviders = await context.DnsProviders.ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanManageServices)]
    public async Task<IActionResult> CreateDnsProvider(ManageDnsProvidersViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.NewName))
        {
            context.DnsProviders.Add(new DnsProvider
            {
                Name = model.NewName,
                Description = model.NewDescription
            });
            await context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(DnsProviders));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanManageServices)]
    public async Task<IActionResult> DeleteDnsProvider(int id)
    {
        var provider = await context.DnsProviders
            .Include(p => p.Services)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        if (provider == null) return NotFound();

        if (provider.Services.Any())
        {
            return BadRequest("Cannot delete a DNS provider that is being used by services.");
        }

        context.DnsProviders.Remove(provider);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(DnsProviders));
    }

    [HttpGet]
    public async Task<IActionResult> GetAssets()
    {
        var assets = await context.Assets
            .OrderBy(a => a.AssetTag)
            .ToListAsync();
        return Json(assets.Select(a => new { a.Id, a.AssetTag }));
    }

    [HttpGet]
    public async Task<IActionResult> GetDnsProviders()
    {
        var providers = await context.DnsProviders
            .OrderBy(p => p.Name)
            .ToListAsync();
        return Json(providers.Select(p => new { p.Id, p.Name }));
    }

    [HttpGet]
    public async Task<IActionResult> GetServices()
    {
        var services = await context.Services
            .OrderBy(s => s.Domain)
            .ToListAsync();
        return Json(services.Select(s => new { s.Id, s.Domain }));
    }

    [HttpGet]
    public async Task<IActionResult> GetLocations()
    {
        var locations = await context.Locations
            .OrderBy(l => l.Name)
            .ToListAsync();
        return Json(locations.Select(l => new { l.Id, l.Name }));
    }

    [HttpGet]
    public async Task<IActionResult> GetCompanyEntities()
    {
        var entities = await context.CompanyEntities
            .OrderBy(e => e.CompanyName)
            .ToListAsync();
        return Json(entities.Select(e => new { e.Id, e.CompanyName }));
    }
}
