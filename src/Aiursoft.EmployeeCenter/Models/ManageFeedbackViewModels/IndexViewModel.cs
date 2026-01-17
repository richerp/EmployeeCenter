using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ManageFeedbackViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel() => PageTitle = "Manage Feedback";
    public List<SignalQuestionnaire> Questionnaires { get; set; } = new();
}
