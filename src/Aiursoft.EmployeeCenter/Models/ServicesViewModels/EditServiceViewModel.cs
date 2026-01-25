using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Models.ServicesViewModels;

public class EditServiceViewModel : CreateServiceViewModel
{
    [Required(ErrorMessage = "The {0} is required.")]
    [Display(Name = "Id")]
    public int Id { get; set; }
}
