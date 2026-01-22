using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Models.ServicesViewModels;

public class EditServiceViewModel : CreateServiceViewModel
{
    [Required]
    public int Id { get; set; }
}
