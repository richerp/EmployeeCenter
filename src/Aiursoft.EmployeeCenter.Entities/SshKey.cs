using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Entities;

public class SshKey
{
    [Key]
    public int Id { get; set; }

    public required string OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    public required string PublicKey { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
}
