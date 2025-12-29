using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;

namespace Aiursoft.EmployeeCenter.Models.PasswordsViewModels;

public class AddShareViewModel
{
    public string? TargetUserId { get; set; }
    public string? TargetRoleId { get; set; }
    
    [Required]
    public SharePermission Permission { get; set; }
}
