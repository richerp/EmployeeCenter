using System.ComponentModel.DataAnnotations;
using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.IntangibleAssetsViewModels;

public class DetailsViewModel : UiStackLayoutViewModel
{
    [Display(Name = "Asset")]
    public required IntangibleAsset Asset { get; set; }
}
