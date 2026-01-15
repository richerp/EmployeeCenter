using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.CompanyEntityViewModels;

public class CreateViewModel : UiStackLayoutViewModel
{
    public CreateViewModel()
    {
        PageTitle = "Create Company Entity";
    }

    [Required]
    [Display(Name = "Company Name (Chinese)")]
    [MaxLength(200)]
    public required string CompanyName { get; set; }

    [Display(Name = "Company Name (English)")]
    [MaxLength(200)]
    public string? CompanyNameEnglish { get; set; }

    [Required]
    [Display(Name = "Entity Code (Social Credit Code / BR Number)")]
    [MaxLength(50)]
    public required string EntityCode { get; set; }

    [Display(Name = "CI Number (HK)")]
    [MaxLength(50)]
    public string? CINumber { get; set; }

    [Display(Name = "Registered Address")]
    [MaxLength(500)]
    public string? RegisteredAddress { get; set; }

    [Display(Name = "Office Address")]
    [MaxLength(500)]
    public string? OfficeAddress { get; set; }

    [Display(Name = "Legal Representative / Director")]
    [MaxLength(100)]
    public string? LegalRepresentative { get; set; }

    [Display(Name = "Company Type")]
    [MaxLength(100)]
    public string? CompanyType { get; set; }

    [Display(Name = "Establishment Date")]
    [DataType(DataType.Date)]
    public DateTime? EstablishmentDate { get; set; }

    [Display(Name = "Expiry Date (Business License / BR)")]
    [DataType(DataType.Date)]
    public DateTime? ExpiryDate { get; set; }

    [Display(Name = "Bank Name")]
    [MaxLength(100)]
    public string? BankName { get; set; }

    [Display(Name = "Bank Account")]
    [MaxLength(50)]
    public string? BankAccount { get; set; }

    [Display(Name = "Bank Account Name")]
    [MaxLength(50)]
    public string? BankAccountName { get; set; }

    [Display(Name = "Company Logo")]
    [RegularExpression(@"^companyfiles.*", ErrorMessage = "Please upload a valid logo file.")]
    public string? LogoPath { get; set; }

    [Display(Name = "Company Seal")]
    [RegularExpression(@"^companyfiles.*", ErrorMessage = "Please upload a valid seal file.")]
    public string? SealPath { get; set; }

    [Display(Name = "Business Registration (BR / Business License)")]
    [RegularExpression(@"^(companyfiles|company-certs)/.*", ErrorMessage = "Please upload a valid license file.")]
    public string? LicensePath { get; set; }

    [Display(Name = "Organization Certificate (CR / Org Code)")]
    [RegularExpression(@"^company-certs/.*", ErrorMessage = "Please upload a valid certificate file.")]
    public string? OrganizationCertificatePath { get; set; }

    [Display(Name = "Registered Capital (Mainland)")]
    [MaxLength(50)]
    public string? RegisteredCapital { get; set; }

    [Display(Name = "Operation Status")]
    [MaxLength(50)]
    public string? OperationStatus { get; set; }

    [Display(Name = "SCR Location (HK)")]
    [MaxLength(500)]
    public string? SCRLocation { get; set; }

    [Display(Name = "Company Secretary (HK)")]
    [MaxLength(100)]
    public string? CompanySecretary { get; set; }
}
