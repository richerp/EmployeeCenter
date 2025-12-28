using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Entities;

public class Payroll
{
    [Key]
    public int Id { get; set; }

    public required string OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public DateTime TargetMonth { get; set; }

    public required string Content { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
}
