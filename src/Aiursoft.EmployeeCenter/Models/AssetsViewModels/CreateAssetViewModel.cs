using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.AssetsViewModels;

public class CreateAssetViewModel : UiStackLayoutViewModel
{
    [Required]
    [Display(Name = "Asset Tag")]
    [MaxLength(50)]
    public string AssetTag { get; set; } = string.Empty;

    [Display(Name = "Serial Number")]
    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    [Required]
    [Display(Name = "Model")]
    public int ModelId { get; set; }

    [Required]
    public AssetStatus Status { get; set; } = AssetStatus.Idle;

    [Display(Name = "Assignee")]
    [MaxLength(255)]
    public string? AssigneeId { get; set; }

    [Display(Name = "Location")]
    public int? LocationId { get; set; }

    [Display(Name = "Belongs to Entity")]
    public int? CompanyEntityId { get; set; }

    [Display(Name = "Purchase Date")]
    public DateTime? PurchaseDate { get; set; }

    [Display(Name = "Purchase Price")]
    public decimal? PurchasePrice { get; set; }

    [Display(Name = "Vendor")]
    public int? VendorId { get; set; }

    [Display(Name = "Warranty Expire Date")]
    public DateTime? WarrantyExpireDate { get; set; }

    [Display(Name = "Invoice File")]
    [MaxLength(500)]
    public string? InvoiceFileUrl { get; set; }

    public bool IsReimbursed { get; set; }

    // Selection lists
    public List<AssetModel> AllModels { get; set; } = new();
    public List<Location> AllLocations { get; set; } = new();
    public List<CompanyEntity> AllCompanyEntities { get; set; } = new();
    public List<Vendor> AllVendors { get; set; } = new();
    public List<User> AllUsers { get; set; } = new();
}
