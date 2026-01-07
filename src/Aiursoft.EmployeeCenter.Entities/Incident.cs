using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aiursoft.EmployeeCenter.Entities;

public enum IncidentSeverity
{
    Sev0,
    Sev1,
    Sev2,
    Sev3,
    Sev4
}

public enum IncidentStatus
{
    Open,
    Mitigated,
    Resolved,
    Closed
}

public class Incident
{
    [Key]
    public Guid Id { get; init; } = Guid.NewGuid();

    [MaxLength(100)]
    public required string Title { get; set; }

    [MaxLength(2000)]
    public required string Description { get; set; }

    public IncidentSeverity Severity { get; set; }

    [MaxLength(100)]
    public required string TargetRole { get; set; }

    [MaxLength(128)]
    public string? OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public User? Owner { get; set; }

    [MaxLength(128)]
    public string? IMId { get; set; }

    [ForeignKey(nameof(IMId))]
    public User? IM { get; set; }

    public IncidentStatus Status { get; set; } = IncidentStatus.Open;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? MitigatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    [MaxLength(2000)]
    public string? MitigationReason { get; set; }

    [MaxLength(2000)]
    public string? ResolutionReason { get; set; }

    [MaxLength(8192)]
    public string? PostMortem { get; set; }

    [InverseProperty(nameof(IncidentComment.Incident))]
    public IEnumerable<IncidentComment> Comments { get; init; } = new List<IncidentComment>();
}
