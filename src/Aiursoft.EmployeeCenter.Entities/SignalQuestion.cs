using System.ComponentModel.DataAnnotations;

namespace Aiursoft.EmployeeCenter.Entities;

public class SignalQuestion
{
    public int Id { get; set; }

    [MaxLength(500)]
    public required string Title { get; set; }

    public SignalQuestionType Type { get; set; }

    [MaxLength(200)]
    public string? Tags { get; set; }

    public string? Meta { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.UtcNow;

    public List<SignalQuestionnaireQuestion> QuestionnaireQuestions { get; init; } = new();
}
