using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.SshKeysViewModels;

public class EditViewModel : UiStackLayoutViewModel
{
    public EditViewModel()
    {
        PageTitle = "Edit SSH Key";
    }

    public int Id { get; set; }
    public string? TargetUserId { get; set; }

    [Required]
    [MaxLength(255)]
    [Display(Name = "Key Name")]
    public string? Name { get; set; }

    [Required]
    [MaxLength(5000)]
    [Display(Name = "Public Key")]
    public string? PublicKey { get; set; }
}
