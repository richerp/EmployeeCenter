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
    TemplateDbContext context)
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

    [Authorize(Policy = AppPermissionNames.CanManagePayroll)]
    [RenderInNavBar(
        NavGroupName = "Administration",
        NavGroupOrder = 9999,
        CascadedLinksGroupName = "Finance",
        CascadedLinksIcon = "shopping-cart",
        CascadedLinksOrder = 9997,
        LinkText = "Manage Payrolls",
        LinkOrder = 1)]
    public async Task<IActionResult> Manage()
    {
        var payrolls = await context.Payrolls
            .Include(p => p.Owner)
            .OrderByDescending(p => p.TargetMonth)
            .ToListAsync();

        return this.StackView(new ManageViewModel
        {
            Payrolls = payrolls
        });
    }

    [Authorize(Policy = AppPermissionNames.CanManagePayroll)]
    public async Task<IActionResult> Create(string? userId = null)
    {
        return this.StackView(new CreateViewModel
        {
            UserId = userId ?? string.Empty,
            Content = string.Empty,
            AllUsers = await context.Users.ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissionNames.CanManagePayroll)]
    public async Task<IActionResult> Create(CreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var payroll = new Payroll
            {
                OwnerId = model.UserId,
                TargetMonth = model.TargetMonth,
                Content = model.Content,

                BaseSalary = model.BaseSalary,
                JobSalary = model.JobSalary,
                PerformanceBonus = model.PerformanceBonus,
                Overtime = model.Overtime,
                FullAttendance = model.FullAttendance,
                OtherAllowances = model.OtherAllowances,

                LateEarly = model.LateEarly,
                SickLeave = model.SickLeave,
                AdministrativeFines = model.AdministrativeFines,

                PensionPersonal = model.PensionPersonal,
                MedicalPersonal = model.MedicalPersonal,
                UnemploymentPersonal = model.UnemploymentPersonal,
                HousingFundPersonal = model.HousingFundPersonal,

                SpecialAdditionalDeduction = model.SpecialAdditionalDeduction,
                PersonalIncomeTax = model.PersonalIncomeTax,

                TotalAmount = model.TotalAmount,
                BankName = model.BankName,
                BankAccount = model.BankAccount,

                PensionCompany = model.PensionCompany,
                MedicalCompany = model.MedicalCompany,
                UnemploymentCompany = model.UnemploymentCompany,
                WorkInjuryCompany = model.WorkInjuryCompany,
                MaternityCompany = model.MaternityCompany,
                HousingFundCompany = model.HousingFundCompany,

                CreationTime = DateTime.UtcNow
            };
            context.Payrolls.Add(payroll);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }

        model.AllUsers = await context.Users.ToListAsync();
        return this.StackView(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var payroll = await context.Payrolls
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payroll == null) return NotFound();

        var user = await userManager.GetUserAsync(User);
        var isFinance = User.HasClaim(AppPermissions.Type, AppPermissionNames.CanManagePayroll);

        if (payroll.OwnerId != user?.Id && !isFinance)
        {
            return Unauthorized();
        }

        return this.StackView(new DetailsViewModel
        {
            Payroll = payroll
        });
    }
}
