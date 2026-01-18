using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.FeedbackViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel() => PageTitle = "Employee Signals";
    public List<SignalQuestionnaire> ActiveQuestionnaires { get; set; } = new();
}
