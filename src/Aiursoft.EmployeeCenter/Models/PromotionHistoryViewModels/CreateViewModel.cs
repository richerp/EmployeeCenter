using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.PromotionHistoryViewModels;

public class CreateViewModel : UiStackLayoutViewModel
{
    public CreateViewModel()
    {
        PageTitle = "Create Promotion";
    }

    [Required]
    [Display(Name = "User")]
    public string? UserId { get; set; }

    [Display(Name = "Old Job Level")]
    public string? OldJobLevel { get; set; }

    [Required]
    [Display(Name = "New Job Level")]
    public string? NewJobLevel { get; set; }

    [Display(Name = "Old Title")]
    public string? OldTitle { get; set; }

    [Required]
    [Display(Name = "New Title")]
    public string? NewTitle { get; set; }

    public string? UserDisplayName { get; set; }
}
