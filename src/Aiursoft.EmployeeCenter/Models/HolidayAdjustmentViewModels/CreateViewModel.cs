using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.HolidayAdjustmentViewModels;

public class CreateViewModel : UiStackLayoutViewModel
{
    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    public HolidayType Type { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
