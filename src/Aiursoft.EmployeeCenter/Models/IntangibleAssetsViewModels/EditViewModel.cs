using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Models.IntangibleAssetsViewModels;

public class EditViewModel : CreateViewModel
{
    [Required]
    public Guid Id { get; set; }
}
