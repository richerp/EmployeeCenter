using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Configuration;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.CertificateViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize]
[LimitPerMin]
public class CertificateController(
    UserManager<User> userManager,
    IOptions<AppSettings> appSettings) : Controller
{
    [HttpGet]
    [RenderInNavBar(
        NavGroupName = "Certificates",
        NavGroupOrder = 5,
        CascadedLinksGroupName = "Print Certificate",
        CascadedLinksIcon = "printer",
        CascadedLinksOrder = 1,
        LinkText = "Certificate of Employment",
        LinkOrder = 1)]
    public async Task<IActionResult> Employment()
    {
        var user = await userManager.GetUserAsync(User);
        return this.StackView(new PrintViewModel
        {
            TargetUser = user,
            Type = CertificateType.Employment,
            CompanyName = appSettings.Value.CompanyName
        });
    }

    [HttpGet]
    [RenderInNavBar(
        NavGroupName = "Certificates",
        NavGroupOrder = 5,
        CascadedLinksGroupName = "Print Certificate",
        CascadedLinksIcon = "printer",
        CascadedLinksOrder = 1,
        LinkText = "Certificate of Income",
        LinkOrder = 2)]
    public async Task<IActionResult> Income()
    {
        var user = await userManager.GetUserAsync(User);
        return this.StackView(new PrintViewModel
        {
            TargetUser = user,
            Type = CertificateType.Income,
            CompanyName = appSettings.Value.CompanyName
        });
    }

    [HttpGet]
    [Authorize(Policy = AppPermissionNames.CanPrintCertificates)]
    [RenderInNavBar(
        NavGroupName = "Administration",
        NavGroupOrder = 9999,
        CascadedLinksGroupName = "Print",
        CascadedLinksIcon = "printer",
        CascadedLinksOrder = 100,
        LinkText = "Manage Printing",
        LinkOrder = 1)]
    public IActionResult Admin()
    {
        return this.StackView(new IndexViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Print(CertificateType type, string lang, string? userId)
    {
        User? targetUser;
        if (string.IsNullOrEmpty(userId))
        {
            targetUser = await userManager.GetUserAsync(User);
        }
        else
        {
            if (!User.HasClaim(AppPermissions.Type, AppPermissionNames.CanPrintCertificates))
            {
                return Forbid();
            }
            targetUser = await userManager.FindByIdAsync(userId);
        }

        if (targetUser == null)
        {
            return NotFound();
        }

        return View(new PrintViewModel
        {
            TargetUser = targetUser,
            Type = type,
            Language = lang,
            CompanyName = appSettings.Value.CompanyName
        });
    }
}
