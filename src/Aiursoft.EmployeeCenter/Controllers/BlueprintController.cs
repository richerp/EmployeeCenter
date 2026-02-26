using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.BlueprintViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.UiStack.Navigation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Localization;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize]
public class BlueprintController(
    EmployeeCenterDbContext dbContext,
    UserManager<User> userManager,
    IStringLocalizer<BlueprintController> localizer) : Controller
{
    [HttpGet]
    [RenderInNavBar(
        NavGroupName = "Career",
        NavGroupOrder = 1,
        CascadedLinksGroupName = "Blueprints",
        CascadedLinksIcon = "map",
        CascadedLinksOrder = 2,
        LinkText = "View Blueprints",
        LinkOrder = 1)]
    public async Task<IActionResult> Index()
    {
        var blueprints = await dbContext.Blueprints
            .Include(t => t.Author)
            .OrderByDescending(t => t.CreationTime)
            .ToListAsync();
        return this.StackView(new IndexViewModel
        {
            Blueprints = blueprints,
            PageTitle = localizer["View Blueprints"]
        });
    }

    [HttpGet]
    [Authorize(Policy = AppPermissionNames.CanManageBlueprints)]
    [RenderInNavBar(
        NavGroupName = "Career",
        NavGroupOrder = 1,
        CascadedLinksGroupName = "Blueprints",
        CascadedLinksIcon = "map",
        CascadedLinksOrder = 2,
        LinkText = "Manage Blueprints",
        LinkOrder = 2)]
    public async Task<IActionResult> Manage()
    {
        var blueprints = await dbContext.Blueprints
            .Include(t => t.Author)
            .OrderByDescending(t => t.CreationTime)
            .ToListAsync();
        return this.StackView(new IndexViewModel
        {
            Blueprints = blueprints,
            PageTitle = localizer["Manage Blueprints"]
        });
    }

    [HttpGet]
    [Authorize(Policy = AppPermissionNames.CanManageBlueprints)]
    public IActionResult Create()
    {
        return this.StackView(new EditorViewModel
        {
            PageTitle = localizer["Create Blueprint"]
        }, "Editor");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanManageBlueprints)]
    public async Task<IActionResult> Create(EditorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.PageTitle = localizer["Create Blueprint"];
            return this.StackView(model, "Editor");
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }
        var html = MarkdownService.RenderMarkdown(model.InputMarkdown).Value ?? string.Empty;

        var blueprint = new Blueprint
        {
            Title = model.Title,
            Content = model.InputMarkdown,
            RenderedHtml = html,
            AuthorId = user.Id,
            CreationTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };

        dbContext.Blueprints.Add(blueprint);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = blueprint.Id, saved = true });
    }

    [HttpGet]
    [Authorize(Policy = AppPermissionNames.CanManageBlueprints)]
    public async Task<IActionResult> Edit(int id, bool saved = false)
    {
        var blueprint = await dbContext.Blueprints.FindAsync(id);
        if (blueprint == null) return NotFound();

        var model = new EditorViewModel
        {
            DocumentId = blueprint.Id,
            Title = blueprint.Title,
            InputMarkdown = blueprint.Content,
            OutputHtml = blueprint.RenderedHtml,
            SavedSuccessfully = saved,
            PageTitle = localizer["Edit Blueprint"]
        };
        return this.StackView(model, "Editor");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanManageBlueprints)]
    public async Task<IActionResult> Edit(EditorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.PageTitle = localizer["Edit Blueprint"];
            return this.StackView(model, "Editor");
        }

        var blueprint = await dbContext.Blueprints.FindAsync(model.DocumentId);
        if (blueprint == null) return NotFound();

        blueprint.Title = model.Title;
        blueprint.Content = model.InputMarkdown;
        blueprint.RenderedHtml = MarkdownService.RenderMarkdown(model.InputMarkdown).Value ?? string.Empty;
        blueprint.UpdateTime = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        model.OutputHtml = blueprint.RenderedHtml;
        model.SavedSuccessfully = true;
        model.PageTitle = localizer["Edit Blueprint"];

        return this.StackView(model, "Editor");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanManageBlueprints)]
    public async Task<IActionResult> Delete(int id)
    {
        var blueprint = await dbContext.Blueprints.FindAsync(id);
        if (blueprint == null) return NotFound();

        dbContext.Blueprints.Remove(blueprint);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Manage));
    }

    [HttpGet]
    public async Task<IActionResult> View(int id)
    {
        var blueprint = await dbContext.Blueprints
            .Include(t => t.Author)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (blueprint == null) return NotFound();

        return this.StackView(new ReaderViewModel
        {
            Blueprint = blueprint,
            PageTitle = blueprint.Title
        });
    }
}
