using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.AssetsViewModels;

public class ManageLocationsViewModel : UiStackLayoutViewModel
{
    public List<Location> Locations { get; set; } = new();
    public string NewName { get; set; } = string.Empty;
}
