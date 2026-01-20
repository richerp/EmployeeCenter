using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Models.ManageFeedbackViewModels;

public class EditQuestionnaireViewModel : CreateQuestionnaireViewModel
{
    [Required(ErrorMessage = "The {0} is required.")]
    [Display(Name = "Id")]
    public int Id { get; set; }
}
