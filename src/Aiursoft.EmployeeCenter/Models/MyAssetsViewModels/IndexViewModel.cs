using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.MyAssetsViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public List<Asset> Assets { get; set; } = new();
}
