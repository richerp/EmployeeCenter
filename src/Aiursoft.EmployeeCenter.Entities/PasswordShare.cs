using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Aiursoft.EmployeeCenter.Entities;

public class PasswordShare
{
    [Key]
    public Guid Id { get; init; } = Guid.NewGuid();

    public required Guid PasswordId { get; set; }

    [JsonIgnore]
    [ForeignKey(nameof(PasswordId))]
    [NotNull]
    public Password? Password { get; set; }

    [StringLength(64)]
    public string? SharedWithUserId { get; set; }

    [JsonIgnore]
    [ForeignKey(nameof(SharedWithUserId))]
    public User? SharedWithUser { get; set; }

    [StringLength(450)]
    public string? SharedWithRoleId { get; set; }

    public required SharePermission Permission { get; set; }

    public DateTime CreationTime { get; init; } = DateTime.UtcNow;
}
