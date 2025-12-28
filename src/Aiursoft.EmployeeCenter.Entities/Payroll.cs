using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Entities;

public class Payroll
{
    [Key]
    public int Id { get; set; }

    public required string OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public DateTime TargetMonth { get; set; }

    public required string Content { get; set; }

    // Earnings
    public decimal BaseSalary { get; set; }
    public decimal JobSalary { get; set; }
    public decimal PerformanceBonus { get; set; }
    public decimal Overtime { get; set; }
    public decimal FullAttendance { get; set; }
    public decimal OtherAllowances { get; set; }

    // Deductions
    public decimal LateEarly { get; set; }
    public decimal SickLeave { get; set; }
    public decimal AdministrativeFines { get; set; }

    // Insurances (Personal)
    public decimal PensionPersonal { get; set; }
    public decimal MedicalPersonal { get; set; }
    public decimal UnemploymentPersonal { get; set; }
    public decimal HousingFundPersonal { get; set; }

    // Tax
    public decimal SpecialAdditionalDeduction { get; set; }
    public decimal PersonalIncomeTax { get; set; }

    // Actual
    public decimal TotalAmount { get; set; }
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }

    // Company Costs
    public decimal PensionCompany { get; set; }
    public decimal MedicalCompany { get; set; }
    public decimal UnemploymentCompany { get; set; }
    public decimal WorkInjuryCompany { get; set; }
    public decimal MaternityCompany { get; set; }
    public decimal HousingFundCompany { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
}
