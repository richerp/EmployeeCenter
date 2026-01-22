using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Models.ServersViewModels;

public class EditServerViewModel : CreateServerViewModel
{
    [Required]
    public int Id { get; set; }
}
