using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.ManageFeedbackViewModels;

public class ResponseDetailViewModel : UiStackLayoutViewModel
{
    public ResponseDetailViewModel() => PageTitle = "Response Detail";
    public SignalResponse Response { get; set; } = null!;
}
