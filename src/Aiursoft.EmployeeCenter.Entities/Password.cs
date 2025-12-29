using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Aiursoft.EmployeeCenter.Entities;

public class Password
{
    [Key]
    public Guid Id { get; init; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public required string Title { get; set; }

    [MaxLength(100)]
    public string? Account { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Secret { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }

    [Required]
    public required string CreatorId { get; set; }

    [ForeignKey(nameof(CreatorId))]
    [JsonIgnore]
    public User? Creator { get; set; }

    public DateTime CreationTime { get; init; } = DateTime.UtcNow;

    [InverseProperty(nameof(PasswordShare.Password))]
    public List<PasswordShare> PasswordShares { get; set; } = new();
}
