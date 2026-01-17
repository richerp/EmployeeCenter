using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Models.ManageFeedbackViewModels;

public class EditQuestionViewModel : CreateQuestionViewModel
{
    [Required]
    public int Id { get; set; }
}
