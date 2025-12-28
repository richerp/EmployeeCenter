using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.PayrollViewModels;

public class CreateViewModel : UiStackLayoutViewModel
{
    public CreateViewModel()
    {
        PageTitle = "Issue Payroll";
    }

    [Required]
    [Display(Name = "User")]
    public required string UserId { get; set; }

    public List<User> AllUsers { get; set; } = [];

    [Required]
    [Display(Name = "Target Month")]
    [DataType(DataType.Date)]
    public DateTime TargetMonth { get; set; } = DateTime.UtcNow;

    [Required]
    [Display(Name = "Content (Markdown)")]
    public string Content { get; set; } = string.Empty;

    // Earnings
    [Display(Name = "Base Salary")]
    public decimal BaseSalary { get; set; }
    [Display(Name = "Job Salary")]
    public decimal JobSalary { get; set; }
    [Display(Name = "Performance Bonus")]
    public decimal PerformanceBonus { get; set; }
    [Display(Name = "Overtime")]
    public decimal Overtime { get; set; }
    [Display(Name = "Full Attendance")]
    public decimal FullAttendance { get; set; }
    [Display(Name = "Other Allowances")]
    public decimal OtherAllowances { get; set; }

    // Deductions
    [Display(Name = "Late/Early")]
    public decimal LateEarly { get; set; }
    [Display(Name = "Sick Leave")]
    public decimal SickLeave { get; set; }
    [Display(Name = "Administrative Fines")]
    public decimal AdministrativeFines { get; set; }

    // Insurances (Personal)
    [Display(Name = "Pension (Personal)")]
    public decimal PensionPersonal { get; set; }
    [Display(Name = "Medical (Personal)")]
    public decimal MedicalPersonal { get; set; }
    [Display(Name = "Unemployment (Personal)")]
    public decimal UnemploymentPersonal { get; set; }
    [Display(Name = "Housing Fund (Personal)")]
    public decimal HousingFundPersonal { get; set; }

    // Tax
    [Display(Name = "Special Additional Deduction")]
    public decimal SpecialAdditionalDeduction { get; set; }
    [Display(Name = "Personal Income Tax")]
    public decimal PersonalIncomeTax { get; set; }

    // Actual
    [Required]
    [Display(Name = "Total Amount")]
    [DataType(DataType.Currency)]
    public decimal TotalAmount { get; set; }
    
    [Display(Name = "Bank Name")]
    public string? BankName { get; set; }
    [Display(Name = "Bank Account")]
    public string? BankAccount { get; set; }

    // Company Costs
    [Display(Name = "Pension (Company)")]
    public decimal PensionCompany { get; set; }
    [Display(Name = "Medical (Company)")]
    public decimal MedicalCompany { get; set; }
    [Display(Name = "Unemployment (Company)")]
    public decimal UnemploymentCompany { get; set; }
    [Display(Name = "Work Injury (Company)")]
    public decimal WorkInjuryCompany { get; set; }
    [Display(Name = "Maternity (Company)")]
    public decimal MaternityCompany { get; set; }
    [Display(Name = "Housing Fund (Company)")]
    public decimal HousingFundCompany { get; set; }
}
