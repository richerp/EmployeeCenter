using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.PayrollViewModels;

public class CreateViewModel : UiStackLayoutViewModel
{
    public CreateViewModel()
    {
        PageTitle = "Issue Payroll";
    }

    [Required]
    [Display(Name = "User")]
    public required string UserId { get; set; }

    public List<User> AllUsers { get; set; } = [];

    [Required]
    [Display(Name = "Target Month")]
    [DataType(DataType.Date)]
    public DateTime TargetMonth { get; set; } = DateTime.UtcNow;

    [Required]
    [Display(Name = "Content (Markdown)")]
    public required string Content { get; set; }

    [Required]
    [Display(Name = "Total Amount")]
    [DataType(DataType.Currency)]
    public decimal TotalAmount { get; set; }
}
