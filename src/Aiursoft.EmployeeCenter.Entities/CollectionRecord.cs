using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aiursoft.EmployeeCenter.Entities;

public class CollectionRecord
{
    [Key]
    public int Id { get; set; }

    public int ChannelId { get; set; }
    [ForeignKey(nameof(ChannelId))]
    public CollectionChannel? Channel { get; set; }

    public decimal ExpectedAmount { get; set; }

    public decimal ActualAmount { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? PaidDate { get; set; }

    [MaxLength(500)]
    public string? ReceiptPath { get; set; }

    [MaxLength(500)]
    public string? InvoicePath { get; set; }

    [MaxLength(200)]
    public string? TransactionId { get; set; }

    [MaxLength(500)]
    public string? SwiftReceiptPath { get; set; }

    public CollectionRecordStatus Status { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
}
