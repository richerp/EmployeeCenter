using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ManageFeedbackViewModels;

public class CreateQuestionViewModel : UiStackLayoutViewModel
{
    public CreateQuestionViewModel() => PageTitle = "Create Question";

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public SignalQuestionType Type { get; set; }

    [MaxLength(200)]
    public string? Tags { get; set; }

    public string? Meta { get; set; }
}
