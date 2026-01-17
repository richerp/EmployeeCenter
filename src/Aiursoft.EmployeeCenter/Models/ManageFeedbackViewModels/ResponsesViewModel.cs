using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ManageFeedbackViewModels;

public class ResponsesViewModel : UiStackLayoutViewModel
{
    public ResponsesViewModel() => PageTitle = "Questionnaire Responses";
    public SignalQuestionnaire Questionnaire { get; set; } = null!;
    public List<SignalResponse> Responses { get; set; } = new();
}
