using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Aiursoft.EmployeeCenter.Entities;

public class User : IdentityUser
{
    public const string DefaultAvatarPath = "Workspace/avatar/default-avatar.jpg";

    [MaxLength(30)]
    [MinLength(2)]
    public required string DisplayName { get; set; }

    [MaxLength(150)] [MinLength(2)] public string AvatarRelativePath { get; set; } = DefaultAvatarPath;

    public DateTime CreationTime { get; init; } = DateTime.UtcNow;

    [MaxLength(30)]
    public string? JobLevel { get; set; }

    public decimal BaseSalary { get; set; }

    [MaxLength(30)]
    public string? BankAccount { get; set; }

    [MaxLength(30)]
    public string? BankName { get; set; }
}
