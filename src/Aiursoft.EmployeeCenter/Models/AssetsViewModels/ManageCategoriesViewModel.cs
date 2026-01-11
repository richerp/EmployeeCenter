using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.AssetsViewModels;

public class ManageCategoriesViewModel : UiStackLayoutViewModel
{
    public List<AssetCategory> Categories { get; set; } = new();
    public string NewName { get; set; } = string.Empty;
    public string NewCode { get; set; } = string.Empty;
}
