using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.IntangibleAssetsViewModels;

public class CreateViewModel : UiStackLayoutViewModel
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public IntangibleAssetType Type { get; set; }

    [MaxLength(200)]
    public string? Supplier { get; set; }

    [MaxLength(100)]
    public string? Account { get; set; }

    [MaxLength(100)]
    public string? Password { get; set; }

    [MaxLength(500)]
    [Url]
    public string? ManagementUrl { get; set; }

    [MaxLength(100)]
    public string? FilingNumber { get; set; }

    [MaxLength(200)]
    public string? FilingSubject { get; set; }

    [MaxLength(1000)]
    public string? FilingQueryMethod { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public decimal? PurchasePrice { get; set; }

    [Display(Name = "Currency")]
    [MaxLength(10)]
    public string Currency { get; set; } = "CNY";

    [MaxLength(500)]
    [Display(Name = "Invoice File")]
    public string? InvoiceFileUrl { get; set; }

    [MaxLength(500)]
    [Display(Name = "Registration Certificate")]
    public string? RegistrationCertificateUrl { get; set; }

    [Display(Name = "Public")]
    public bool IsPublic { get; set; }

    [Display(Name = "Belongs to Entity")]
    public int? CompanyEntityId { get; set; }

    [MaxLength(255)]
    public string? AssigneeId { get; set; }

    [Required]
    public IntangibleAssetStatus Status { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public IEnumerable<User> AllUsers { get; set; } = new List<User>();

    public IEnumerable<CompanyEntity> AllCompanyEntities { get; set; } = new List<CompanyEntity>();

    public Dictionary<string, string> CurrencyOptions { get; set; } = new();
}
