using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.BlueprintViewModels;

public class ReaderViewModel : UiStackLayoutViewModel
{
    public ReaderViewModel()
    {
        PageTitle = "Blueprint";
    }
    public required Blueprint Blueprint { get; set; }
}
