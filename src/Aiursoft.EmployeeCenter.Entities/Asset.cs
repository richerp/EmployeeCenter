using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Aiursoft.EmployeeCenter.Entities;

public enum AssetStatus
{
    Idle = 1,
    PendingAccept = 2,
    InUse = 3,
    Maintenance = 4,
    Retired = 5,
    Lost = 6
}

public class Asset
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string AssetTag { get; set; }

    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    [Required]
    public int ModelId { get; set; }

    [ForeignKey(nameof(ModelId))]
    public AssetModel Model { get; set; } = null!;

    [Required]
    public AssetStatus Status { get; set; }

    [MaxLength(255)]
    public string? AssigneeId { get; set; }

    [ForeignKey(nameof(AssigneeId))]
    public User? Assignee { get; set; }

    public int? LocationId { get; set; }

    [ForeignKey(nameof(LocationId))]
    public Location? Location { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public decimal? PurchasePrice { get; set; }

    public int? VendorId { get; set; }

    [ForeignKey(nameof(VendorId))]
    public Vendor? Vendor { get; set; }

    public DateTime? WarrantyExpireDate { get; set; }

    [MaxLength(500)]
    public string? InvoiceFileUrl { get; set; }

    public bool IsReimbursed { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    [JsonIgnore]
    public List<AssetHistory> Histories { get; set; } = new();
}
