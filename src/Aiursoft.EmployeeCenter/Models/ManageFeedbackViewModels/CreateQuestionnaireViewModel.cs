using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Aiursoft.EmployeeCenter.Models.ManageFeedbackViewModels;

public class CreateQuestionnaireViewModel : UiStackLayoutViewModel
{
    public CreateQuestionnaireViewModel() => PageTitle = "Create Questionnaire";

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public List<int> SelectedQuestionIds { get; set; } = new();

    [ValidateNever]
    public List<SignalQuestion> AllAvailableQuestions { get; set; } = new();
}
