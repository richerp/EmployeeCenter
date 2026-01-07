using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.LeaveViewModels;

public class ApplyViewModel : UiStackLayoutViewModel
{
    [Required]
    public LeaveType LeaveType { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Calculated total working days (set by controller after validation)
    /// </summary>
    public decimal TotalDays { get; set; }
}
