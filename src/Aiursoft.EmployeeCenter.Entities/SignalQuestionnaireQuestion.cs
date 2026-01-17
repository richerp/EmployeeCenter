namespace Aiursoft.EmployeeCenter.Entities;

public class SignalQuestionnaireQuestion
{
    public int Id { get; set; }
    public int QuestionnaireId { get; set; }
    public SignalQuestionnaire Questionnaire { get; set; } = null!;

    public int QuestionId { get; set; }
    public SignalQuestion Question { get; set; } = null!;

    public int Order { get; set; }
}
