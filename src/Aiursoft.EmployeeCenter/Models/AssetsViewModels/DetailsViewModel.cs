using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.AssetsViewModels;

public class DetailsViewModel : UiStackLayoutViewModel
{
    public Asset Asset { get; set; } = null!;
}
