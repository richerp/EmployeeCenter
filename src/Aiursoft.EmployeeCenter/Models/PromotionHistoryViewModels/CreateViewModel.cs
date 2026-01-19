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
    [MaxLength(100)]
    public string? OldJobLevel { get; set; }

    [Required]
    [Display(Name = "New Job Level")]
    [MaxLength(100)]
    public string? NewJobLevel { get; set; }

    [Display(Name = "Old Title")]
    [MaxLength(100)]
    public string? OldTitle { get; set; }

    [Required]
    [Display(Name = "New Title")]
    [MaxLength(100)]
    public string? NewTitle { get; set; }

    public string? UserDisplayName { get; set; }
}
