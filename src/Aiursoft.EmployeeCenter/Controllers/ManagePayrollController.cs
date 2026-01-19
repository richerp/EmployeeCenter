using Aiursoft.EmployeeCenter.Authorization;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.EmployeeCenter.Models.PayrollViewModels;
using Aiursoft.EmployeeCenter.Services;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aiursoft.CSTools.Tools;

namespace Aiursoft.EmployeeCenter.Controllers;

[Authorize(Policy = AppPermissionNames.CanManagePayroll)]
[LimitPerMin]
public class ManagePayrollController(
    EmployeeCenterDbContext context)
    : Controller
{
    [RenderInNavBar(
        NavGroupName = "Administration",
        NavGroupOrder = 3,
        CascadedLinksGroupName = "Finance",
        CascadedLinksIcon = "shopping-cart",
        CascadedLinksOrder = 2,
        LinkText = "Manage Payrolls",
        LinkOrder = 1)]
    public async Task<IActionResult> Index()
    {
        var payrolls = await context.Payrolls
            .Include(p => p.Owner)
            .OrderByDescending(p => p.TargetMonth)
            .ToListAsync();

        var users = await context.Users
            .OrderBy(u => u.UserName)
            .ToListAsync();

        return this.StackView(new ManageViewModel
        {
            Payrolls = payrolls,
            AllUsers = users
        });
    }

    [HttpGet]
    public async Task<IActionResult> Export(string? userId, int? year)
    {
        var query = context.Payrolls.Include(t => t.Owner).AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(t => t.OwnerId == userId);
        }

        if (year.HasValue)
        {
            query = query.Where(t => t.TargetMonth.Year == year.Value);
        }

        var list = await query
            .OrderByDescending(p => p.TargetMonth)
            .ToListAsync();

        var exportList = list.Select(p => new PayrollExportViewModel
        {
            Id = p.Id,
            OwnerId = p.OwnerId,
            OwnerName = p.Owner.UserName ?? string.Empty,
            TargetMonth = p.TargetMonth,
            Content = p.Content,
            BaseSalary = p.BaseSalary,
            JobSalary = p.JobSalary,
            PerformanceBonus = p.PerformanceBonus,
            Overtime = p.Overtime,
            FullAttendance = p.FullAttendance,
            OtherAllowances = p.OtherAllowances,
            LateEarly = p.LateEarly,
            SickLeave = p.SickLeave,
            AdministrativeFines = p.AdministrativeFines,
            PensionPersonal = p.PensionPersonal,
            MedicalPersonal = p.MedicalPersonal,
            UnemploymentPersonal = p.UnemploymentPersonal,
            HousingFundPersonal = p.HousingFundPersonal,
            SpecialAdditionalDeduction = p.SpecialAdditionalDeduction,
            PersonalIncomeTax = p.PersonalIncomeTax,
            TotalAmount = p.TotalAmount,
            BankName = p.BankName,
            BankAccount = p.BankAccount,
            PensionCompany = p.PensionCompany,
            MedicalCompany = p.MedicalCompany,
            UnemploymentCompany = p.UnemploymentCompany,
            WorkInjuryCompany = p.WorkInjuryCompany,
            MaternityCompany = p.MaternityCompany,
            HousingFundCompany = p.HousingFundCompany,
            CreationTime = p.CreationTime
        }).ToList();

        var csv = exportList.ToCsv();
        return File(csv, "text/csv", $"payroll-export-{DateTime.UtcNow:yyyy-MM-dd}.csv");
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
