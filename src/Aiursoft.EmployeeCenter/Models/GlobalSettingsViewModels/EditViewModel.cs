using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Models.GlobalSettingsViewModels;

public class EditViewModel
{
    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Value { get; set; }
}
