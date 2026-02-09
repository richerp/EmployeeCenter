using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.IntangibleAssetsViewModels;

public class DetailsViewModel : UiStackLayoutViewModel
{
    public required IntangibleAsset Asset { get; set; }
}
