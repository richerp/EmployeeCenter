using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.InvoiceViewModels;

public enum InvoiceType
{
    [Display(Name = "Receipt / Paid Invoice")]
    Receipt,
    
    [Display(Name = "Proforma Invoice / Billing Statement")]
    Proforma
}

public class CreateViewModel : UiStackLayoutViewModel
{
    [Required]
    public InvoiceType Type { get; set; } = InvoiceType.Receipt;

    [Required]
    public int SellerId { get; set; }
    
    [Required]
    public string SellerName { get; set; } = string.Empty;
    
    [Required]
    public string SellerAddress { get; set; } = string.Empty;
    
    public string? SellerCode { get; set; }
    
    public string? SellerContact { get; set; }
    
    public int? BuyerId { get; set; }
    
    [Required]
    public string BuyerName { get; set; } = string.Empty;
    
    [Required]
    public string BuyerAddress { get; set; } = string.Empty;
    
    public string? BuyerCRNo { get; set; }
    public string? BuyerAttn { get; set; }
    
    [Required]
    public string InvoiceNo { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;
    
    public string? ContractNo { get; set; }
    
    [Required]
    public string Currency { get; set; } = "HKD";
    
    public List<InvoiceItemViewModel> Items { get; set; } = new();
    
    [Required]
    public string BankName { get; set; } = string.Empty;
    
    [Required]
    public string BankAccount { get; set; } = string.Empty;
    
    [Required]
    public string BeneficiaryName { get; set; } = string.Empty;
    
    public string? SwiftCode { get; set; }
    public string? BankAddress { get; set; }
    
    public decimal Subtotal => Items.Sum(i => i.Quantity * i.UnitPrice);
    public decimal Tax { get; set; }
    public decimal TotalDue => Subtotal + Tax;
}
