using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.PayrollViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize(Policy = AppPermissionNames.CanManagePayroll)]
[LimitPerMin]
public class ManagePayrollController(
    EmployeeCenterDbContext context)
    : Controller
{
    [RenderInNavBar(
        NavGroupName = "Administration",
        NavGroupOrder = 9999,
        CascadedLinksGroupName = "Finance",
        CascadedLinksIcon = "shopping-cart",
        CascadedLinksOrder = 9997,
        LinkText = "Manage Payrolls",
        LinkOrder = 1)]
    public async Task<IActionResult> Index()
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
            return RedirectToAction(nameof(Index));
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

        return this.StackView(new DetailsViewModel
        {
            Payroll = payroll
        });
    }
}
