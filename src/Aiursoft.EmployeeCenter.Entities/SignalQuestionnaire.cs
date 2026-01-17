using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Entities;

public class SignalQuestionnaire
{
    public int Id { get; set; }

    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.UtcNow;

    public List<SignalQuestionnaireQuestion> Questions { get; init; } = new();

    public List<SignalResponse> Responses { get; init; } = new();
}
