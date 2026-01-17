using System.ComponentModel.DataAnnotations.Schema;

namespace Aiursoft.EmployeeCenter.Entities;

public class SignalQuestionResponse
{
    public int Id { get; set; }

    public int SignalResponseId { get; set; }
    [ForeignKey(nameof(SignalResponseId))]
    public SignalResponse SignalResponse { get; set; } = null!;

    public int QuestionId { get; set; }
    [ForeignKey(nameof(QuestionId))]
    public SignalQuestion Question { get; set; } = null!;

    public string? Answer { get; set; }
}
