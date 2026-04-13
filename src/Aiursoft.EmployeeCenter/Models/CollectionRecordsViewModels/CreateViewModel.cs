using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.CollectionRecordsViewModels;

public class CreateViewModel : UiStackLayoutViewModel
{
    [Required]
    public int ChannelId { get; set; }

    [Required]
    [Display(Name = "Expected Amount")]
    public decimal ExpectedAmount { get; set; }

    [Required]
    [Display(Name = "Actual Amount")]
    public decimal ActualAmount { get; set; }

    [Required]
    [Display(Name = "Due Date")]
    public DateTime DueDate { get; set; } = DateTime.Today;

    [Display(Name = "Paid Date")]
    public DateTime? PaidDate { get; set; }

    [MaxLength(500)]
    [Display(Name = "Receipt File")]
    public string? ReceiptPath { get; set; }

    [MaxLength(500)]
    [Display(Name = "Invoice File")]
    public string? InvoicePath { get; set; }

    [MaxLength(200)]
    [Display(Name = "Transaction ID")]
    public string? TransactionId { get; set; }

    [MaxLength(500)]
    [Display(Name = "SWIFT / Bank Receipt File")]
    public string? SwiftReceiptPath { get; set; }

    [Required]
    [Display(Name = "Status")]
    public CollectionRecordStatus Status { get; set; }

    public CollectionChannel? Channel { get; set; }
}
