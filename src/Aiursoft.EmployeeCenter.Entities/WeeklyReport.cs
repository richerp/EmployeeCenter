using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aiursoft.EmployeeCenter.Entities;

public class WeeklyReport
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime WeekStartDate { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
}
