using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.WeeklyReportViewModels;

public class EditViewModel : UiStackLayoutViewModel
{
    public EditViewModel()
    {
        PageTitle = "Edit Weekly Report";
    }

    [Required]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Content")]
    public string Content { get; set; } = string.Empty;

    public DateTime WeekStartDate { get; set; }
    
    public User? User { get; set; }
}
