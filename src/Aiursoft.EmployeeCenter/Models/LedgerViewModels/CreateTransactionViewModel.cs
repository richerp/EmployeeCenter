using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LedgerViewModels;

public class CreateTransactionViewModel : UiStackLayoutViewModel
{
    public int EntityId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int SourceAccountId { get; set; }

    [Required]
    public int DestinationAccountId { get; set; }

    [Required]
    [Range(0.0001, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [Range(0.0000001, double.MaxValue)]
    public decimal ExchangeRate { get; set; } = 1;

    public string? InvoicePath { get; set; }

    [Required]
    public DateTime TransactionTime { get; set; } = DateTime.UtcNow;
}
