using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Models.CompanyEntityViewModels;

public class EditViewModel : CreateViewModel
{
    public EditViewModel()
    {
        PageTitle = "Edit Company Entity";
    }

    [Required]
    public int Id { get; set; }
}
