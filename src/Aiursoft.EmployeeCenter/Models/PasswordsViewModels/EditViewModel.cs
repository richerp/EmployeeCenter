using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.PasswordsViewModels;

public class EditViewModel : UiStackLayoutViewModel
{
    public EditViewModel()
    {
        PageTitle = "Edit Password";
    }

    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string? Title { get; set; }

    [MaxLength(100)]
    public string? Account { get; set; }

    [Required]
    [MaxLength(100)]
    public string? Secret { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }
}
