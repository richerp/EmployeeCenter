using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Models.IntangibleAssetsViewModels;

public class EditViewModel : CreateViewModel
{
    [Required(ErrorMessage = "The {0} is required.")]
    [Display(Name = "Id")]
    public Guid Id { get; set; }
}
