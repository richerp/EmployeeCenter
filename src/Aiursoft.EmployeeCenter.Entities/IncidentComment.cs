using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aiursoft.EmployeeCenter.Entities;

public class IncidentComment
{
    [Key]
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid IncidentId { get; set; }

    [ForeignKey(nameof(IncidentId))]
    public Incident? Incident { get; set; }

    [MaxLength(128)]
    public string? AuthorId { get; set; }

    [ForeignKey(nameof(AuthorId))]
    public User? Author { get; set; }

    [MaxLength(2000)]
    public required string Content { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public bool IsSystemComment { get; set; }
}
