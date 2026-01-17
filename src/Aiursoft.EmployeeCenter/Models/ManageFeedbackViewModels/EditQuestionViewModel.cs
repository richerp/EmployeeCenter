using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;

namespace Aiursoft.EmployeeCenter.Models.ManageFeedbackViewModels;

public class EditQuestionViewModel : CreateQuestionViewModel
{
    [Required]
    public int Id { get; set; }
}
