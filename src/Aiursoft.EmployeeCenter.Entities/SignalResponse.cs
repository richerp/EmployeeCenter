using System.ComponentModel.DataAnnotations.Schema;

namespace Aiursoft.EmployeeCenter.Entities;

public class SignalResponse
{
    public int Id { get; set; }

    public int QuestionnaireId { get; set; }
    [ForeignKey(nameof(QuestionnaireId))]
    public SignalQuestionnaire Questionnaire { get; set; } = null!;

    public required string UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public DateTime SubmitTime { get; set; } = DateTime.UtcNow;

    public List<SignalQuestionResponse> QuestionResponses { get; init; } = new();
}
