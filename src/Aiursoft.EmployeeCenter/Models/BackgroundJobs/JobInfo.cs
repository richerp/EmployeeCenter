using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Models.BackgroundJobs;

/// <summary>
/// Represents a background job in the queue system.
/// </summary>
public class JobInfo
{
    [Display(Name = "Job Id")]
    public Guid JobId { get; init; } = Guid.NewGuid();

    [Display(Name = "Queue Name")]
    public required string QueueName { get; init; }

    [Display(Name = "Job Name")]
    public required string JobName { get; init; }

    [Display(Name = "Status")]
    public JobStatus Status { get; set; } = JobStatus.Pending;

    [Display(Name = "Queued At")]
    public DateTime QueuedAt { get; init; } = DateTime.UtcNow;

    [Display(Name = "Started At")]
    public DateTime? StartedAt { get; set; }

    [Display(Name = "Completed At")]
    public DateTime? CompletedAt { get; set; }

    [Display(Name = "Error Message")]
    public string? ErrorMessage { get; set; }

    [Display(Name = "Service Type")]
    public required Type ServiceType { get; init; }

    [Display(Name = "Job Action")]
    public required Func<object, Task> JobAction { get; init; }
}
