using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Models.ManageFeedbackViewModels;

public class EditQuestionnaireViewModel : CreateQuestionnaireViewModel
{
    [Required]
    public int Id { get; set; }
}
