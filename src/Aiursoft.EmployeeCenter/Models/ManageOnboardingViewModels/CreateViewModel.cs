using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ManageOnboardingViewModels;

public class CreateViewModel : UiStackLayoutViewModel
{
    public CreateViewModel() => PageTitle = "Create Onboarding Task";

    [Required]
    public int Order { get; set; }

    [Required]
    [Display(Name = "Expected Duration (Seconds)")]
    public int ExpectedDurationSeconds { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Url]
    [Display(Name = "Start Link")]
    public string StartLink { get; set; } = string.Empty;
}
