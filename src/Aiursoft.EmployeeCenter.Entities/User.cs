using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Aiursoft.EmployeeCenter.Entities;

public class User : IdentityUser
{
    public const string DefaultAvatarPath = "Workspace/avatar/default-avatar.jpg";

    [MaxLength(30)]
    [MinLength(2)]
    public required string DisplayName { get; set; }

    [MaxLength(150)]
    [MinLength(2)]
    public required string AvatarRelativePath { get; set; } = DefaultAvatarPath;

    public DateTime CreationTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The job level of this user.
    /// If null, it means the user has no job level assigned yet.
    /// </summary>
    [MaxLength(30)]
    public string? JobLevel { get; set; }

    public decimal BaseSalary { get; set; }

    /// <summary>
    /// The bank account of this user.
    /// If null, it means the user has not provided a bank account yet.
    /// </summary>
    [MaxLength(30)]
    public string? BankAccount { get; set; }

    /// <summary>
    /// The bank name of this user.
    /// If null, it means the user has not provided a bank name yet.
    /// </summary>
    [MaxLength(30)]
    public string? BankName { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [InverseProperty(nameof(Password.Creator))]
    public IEnumerable<Password> CreatedPasswords { get; init; } = new List<Password>();

    [Newtonsoft.Json.JsonIgnore]
    [InverseProperty(nameof(PasswordShare.SharedWithUser))]
    public IEnumerable<PasswordShare> PasswordsSharedWithMe { get; init; } = new List<PasswordShare>();

    [Newtonsoft.Json.JsonIgnore]
    [InverseProperty(nameof(Payroll.Owner))]
    public IEnumerable<Payroll> Payrolls { get; init; } = new List<Payroll>();

    [Newtonsoft.Json.JsonIgnore]
    [InverseProperty(nameof(SshKey.Owner))]
    public IEnumerable<SshKey> SshKeys { get; init; } = new List<SshKey>();
}
