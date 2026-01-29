namespace Aiursoft.EmployeeCenter.Models.InvoiceViewModels;

public class InvoiceItemViewModel
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}
