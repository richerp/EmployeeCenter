using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.AssetsViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize(Policy = AppPermissionNames.CanManageAssets)]
[LimitPerMin]
public class AssetsController(
    EmployeeCenterDbContext context,
    UserManager<User> userManager)
    : Controller
{
    [RenderInNavBar(
        NavGroupName = "Administration",
        NavGroupOrder = 3,
        CascadedLinksGroupName = "IT Assets",
        CascadedLinksIcon = "monitor",
        CascadedLinksOrder = 4,
        LinkText = "Manage Assets",
        LinkOrder = 1)]
    public async Task<IActionResult> Index()
    {
        var assets = await context.Assets
            .Include(a => a.Model)
            .ThenInclude(m => m.Category)
            .Include(a => a.Assignee)
            .Include(a => a.Location)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return this.StackView(new IndexViewModel
        {
            Assets = assets
        });
    }

    public async Task<IActionResult> Create()
    {
        return this.StackView(new CreateAssetViewModel
        {
            AllModels = await context.AssetModels.Include(m => m.Category).ToListAsync(),
            AllLocations = await context.Locations.ToListAsync(),
            AllVendors = await context.Vendors.ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAssetViewModel model)
    {
        if (await context.Assets.AnyAsync(a => a.AssetTag == model.AssetTag))
        {
            ModelState.AddModelError(nameof(model.AssetTag), "Asset tag already exists.");
        }

        if (ModelState.IsValid)
        {
            var asset = new Asset
            {
                Id = Guid.NewGuid(),
                AssetTag = model.AssetTag,
                SerialNumber = model.SerialNumber,
                ModelId = model.ModelId,
                Status = model.Status,
                LocationId = model.LocationId,
                PurchaseDate = model.PurchaseDate,
                PurchasePrice = model.PurchasePrice,
                VendorId = model.VendorId,
                WarrantyExpireDate = model.WarrantyExpireDate,
                InvoiceFileUrl = model.InvoiceFileUrl,
                IsReimbursed = model.IsReimbursed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Assets.Add(asset);

            var user = await userManager.GetUserAsync(User);
            await LogHistory(asset.Id, "CREATE", null, null, user?.Id ?? "System", "Asset created.");

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        model.AllModels = await context.AssetModels.Include(m => m.Category).ToListAsync();
        model.AllLocations = await context.Locations.ToListAsync();
        model.AllVendors = await context.Vendors.ToListAsync();
        return this.StackView(model);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var asset = await context.Assets
            .Include(a => a.Model)
            .ThenInclude(m => m.Category)
            .Include(a => a.Assignee)
            .Include(a => a.Location)
            .Include(a => a.Vendor)
            .Include(a => a.Histories)
            .ThenInclude(h => h.Operator)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null) return NotFound();

        return this.StackView(new DetailsViewModel
        {
            Asset = asset
        });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var asset = await context.Assets.FindAsync(id);
        if (asset == null) return NotFound();

        return this.StackView(new EditAssetViewModel
        {
            Id = asset.Id,
            AssetTag = asset.AssetTag,
            SerialNumber = asset.SerialNumber,
            ModelId = asset.ModelId,
            Status = asset.Status,
            LocationId = asset.LocationId,
            PurchaseDate = asset.PurchaseDate,
            PurchasePrice = asset.PurchasePrice,
            VendorId = asset.VendorId,
            WarrantyExpireDate = asset.WarrantyExpireDate,
            InvoiceFileUrl = asset.InvoiceFileUrl,
            IsReimbursed = asset.IsReimbursed,
            AllModels = await context.AssetModels.Include(m => m.Category).ToListAsync(),
            AllLocations = await context.Locations.ToListAsync(),
            AllVendors = await context.Vendors.ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditAssetViewModel model)
    {
        var asset = await context.Assets.FindAsync(model.Id);
        if (asset == null) return NotFound();

        if (await context.Assets.AnyAsync(a => a.AssetTag == model.AssetTag && a.Id != model.Id))
        {
            ModelState.AddModelError(nameof(model.AssetTag), "Asset tag already exists.");
        }

        if (ModelState.IsValid)
        {
            var user = await userManager.GetUserAsync(User);
            var userId = user?.Id ?? "System";

            // Track changes for history
            if (asset.AssetTag != model.AssetTag) await LogHistory(asset.Id, "UPDATE", "AssetTag", asset.AssetTag, userId, "Updated asset tag.");
            if (asset.SerialNumber != model.SerialNumber) await LogHistory(asset.Id, "UPDATE", "SerialNumber", asset.SerialNumber, userId, "Updated serial number.");
            if (asset.ModelId != model.ModelId) await LogHistory(asset.Id, "UPDATE", "ModelId", asset.ModelId.ToString(), userId, "Updated model.");
            if (asset.Status != model.Status) await LogHistory(asset.Id, "UPDATE", "Status", asset.Status.ToString(), userId, "Updated status.");
            if (asset.LocationId != model.LocationId) await LogHistory(asset.Id, "UPDATE", "LocationId", asset.LocationId?.ToString(), userId, "Updated location.");
            if (asset.PurchaseDate != model.PurchaseDate) await LogHistory(asset.Id, "UPDATE", "PurchaseDate", asset.PurchaseDate?.ToString(), userId, "Updated purchase date.");
            if (asset.PurchasePrice != model.PurchasePrice) await LogHistory(asset.Id, "UPDATE", "PurchasePrice", asset.PurchasePrice?.ToString(), userId, "Updated purchase price.");
            if (asset.VendorId != model.VendorId) await LogHistory(asset.Id, "UPDATE", "VendorId", asset.VendorId?.ToString(), userId, "Updated vendor.");
            if (asset.WarrantyExpireDate != model.WarrantyExpireDate) await LogHistory(asset.Id, "UPDATE", "WarrantyExpireDate", asset.WarrantyExpireDate?.ToString(), userId, "Updated warranty expire date.");
            if (asset.InvoiceFileUrl != model.InvoiceFileUrl) await LogHistory(asset.Id, "UPDATE", "InvoiceFileUrl", asset.InvoiceFileUrl, userId, "Updated invoice file.");
            if (asset.IsReimbursed != model.IsReimbursed) await LogHistory(asset.Id, "UPDATE", "IsReimbursed", asset.IsReimbursed.ToString(), userId, "Updated reimbursement status.");

            asset.AssetTag = model.AssetTag;
            asset.SerialNumber = model.SerialNumber;
            asset.ModelId = model.ModelId;
            asset.Status = model.Status;
            asset.LocationId = model.LocationId;
            asset.PurchaseDate = model.PurchaseDate;
            asset.PurchasePrice = model.PurchasePrice;
            asset.VendorId = model.VendorId;
            asset.WarrantyExpireDate = model.WarrantyExpireDate;
            asset.InvoiceFileUrl = model.InvoiceFileUrl;
            asset.IsReimbursed = model.IsReimbursed;
            asset.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        model.AllModels = await context.AssetModels.Include(m => m.Category).ToListAsync();
        model.AllLocations = await context.Locations.ToListAsync();
        model.AllVendors = await context.Vendors.ToListAsync();
        return this.StackView(model);
    }

    public async Task<IActionResult> Assign(Guid id)
    {
        var asset = await context.Assets
            .Include(a => a.Model)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (asset == null) return NotFound();

        if (asset.Status != AssetStatus.Idle)
        {
            return BadRequest("Asset is not in idle status and cannot be assigned.");
        }

        return this.StackView(new AssignAssetViewModel
        {
            AssetId = asset.Id,
            AssetTag = asset.AssetTag,
            ModelName = asset.Model.ModelName,
            AllUsers = await context.Users.ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(AssignAssetViewModel model)
    {
        var asset = await context.Assets.FindAsync(model.AssetId);
        if (asset == null) return NotFound();

        if (asset.Status != AssetStatus.Idle)
        {
            return BadRequest("Asset is not in idle status and cannot be assigned.");
        }

        if (ModelState.IsValid)
        {
            var user = await userManager.GetUserAsync(User);
            var userId = user?.Id ?? "System";

            asset.Status = AssetStatus.PendingAccept;
            asset.AssigneeId = model.AssigneeId;
            asset.UpdatedAt = DateTime.UtcNow;

            await LogHistory(asset.Id, "ASSIGN", "AssigneeId", null, userId, $"Assigned to user {model.AssigneeId}. Notes: {model.Notes}");

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        model.AllUsers = await context.Users.ToListAsync();
        return this.StackView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(Guid id, int? locationId, string? reason)
    {
        var asset = await context.Assets.FindAsync(id);
        if (asset == null) return NotFound();

        if (asset.Status != AssetStatus.InUse && asset.Status != AssetStatus.PendingAccept)
        {
            return BadRequest("Asset is not in use or pending accept and cannot be returned.");
        }

        var user = await userManager.GetUserAsync(User);
        var userId = user?.Id ?? "System";

        asset.Status = AssetStatus.Idle;
        asset.AssigneeId = null;
        if (locationId.HasValue)
        {
            asset.LocationId = locationId.Value;
        }
        asset.UpdatedAt = DateTime.UtcNow;

        await LogHistory(asset.Id, "RETURN", "Status", AssetStatus.InUse.ToString(), userId, $"Returned to stock. Reason: {reason}");

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LogHistory(Guid assetId, string actionType, string? fieldName, string? oldValue, string operatorId, string? reason)
    {
        var asset = await context.Assets.FindAsync(assetId);
        var newValue = fieldName switch
        {
            "AssetTag" => asset?.AssetTag,
            "SerialNumber" => asset?.SerialNumber,
            "ModelId" => asset?.ModelId.ToString(),
            "Status" => asset?.Status.ToString(),
            "AssigneeId" => asset?.AssigneeId,
            "LocationId" => asset?.LocationId?.ToString(),
            "PurchaseDate" => asset?.PurchaseDate?.ToString(),
            "PurchasePrice" => asset?.PurchasePrice?.ToString(),
            "VendorId" => asset?.VendorId?.ToString(),
            "WarrantyExpireDate" => asset?.WarrantyExpireDate?.ToString(),
            "InvoiceFileUrl" => asset?.InvoiceFileUrl,
            "IsReimbursed" => asset?.IsReimbursed.ToString(),
            _ => null
        };

        var history = new AssetHistory
        {
            AssetId = assetId,
            ActionType = actionType,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            OperatorId = operatorId,
            Reason = reason,
            Timestamp = DateTime.UtcNow
        };
        context.AssetHistories.Add(history);
    }

    // Category Management
    public async Task<IActionResult> Categories()
    {
        return this.StackView(new ManageCategoriesViewModel
        {
            Categories = await context.AssetCategories.ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(ManageCategoriesViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.NewName) && !string.IsNullOrWhiteSpace(model.NewCode))
        {
            context.AssetCategories.Add(new AssetCategory { Name = model.NewName, Code = model.NewCode });
            await context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Categories));
    }

    // Model Management
    public async Task<IActionResult> Models()
    {
        return this.StackView(new ManageModelsViewModel
        {
            Models = await context.AssetModels.Include(m => m.Category).ToListAsync(),
            AllCategories = await context.AssetCategories.ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateModel(ManageModelsViewModel model)
    {
        if (ModelState.IsValid)
        {
            context.AssetModels.Add(new AssetModel
            {
                CategoryId = model.NewCategoryId,
                Brand = model.NewBrand,
                ModelName = model.NewModelName,
                Specs = model.NewSpecs
            });
            await context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Models));
    }

    // Location Management
    public async Task<IActionResult> Locations()
    {
        return this.StackView(new ManageLocationsViewModel
        {
            Locations = await context.Locations.ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLocation(ManageLocationsViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.NewName))
        {
            context.Locations.Add(new Location { Name = model.NewName });
            await context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Locations));
    }

    // Vendor Management
    public async Task<IActionResult> Vendors()
    {
        return this.StackView(new ManageVendorsViewModel
        {
            Vendors = await context.Vendors.ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVendor(ManageVendorsViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.NewName))
        {
            context.Vendors.Add(new Vendor { Name = model.NewName, ContactInfo = model.NewContactInfo });
            await context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Vendors));
    }
}
