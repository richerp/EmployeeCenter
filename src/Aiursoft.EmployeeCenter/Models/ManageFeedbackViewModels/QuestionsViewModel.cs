using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ManageFeedbackViewModels;

public class QuestionsViewModel : UiStackLayoutViewModel
{
    public QuestionsViewModel() => PageTitle = "Manage Questions";
    public List<SignalQuestion> Questions { get; set; } = new();
}
