using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Entities;

public class Contract
{
    [Key]
    public int Id { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public required string FilePath { get; set; }

    public ContractStatus Status { get; set; }

    public bool IsPublic { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
}
