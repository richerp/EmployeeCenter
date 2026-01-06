using System.ComponentModel.DataAnnotations.Schema;

namespace Aiursoft.EmployeeCenter.Entities;

public class BankCardChangeLog
{
    public int Id { get; set; }
    
    public required string UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public string? OldBankCardNumber { get; set; }
    public string? NewBankCardNumber { get; set; }

    public string? OldBankName { get; set; }
    public string? NewBankName { get; set; }

    public string? OldBankAccountName { get; set; }
    public string? NewBankAccountName { get; set; }

    public DateTime ChangeTime { get; set; } = DateTime.UtcNow;
    
    public string? ChangedByUserId { get; set; }
    
    [ForeignKey(nameof(ChangedByUserId))]
    public User? ChangedByUser { get; set; }
}
