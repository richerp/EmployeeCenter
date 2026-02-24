using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.HolidayAdjustmentViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public required List<AdjustedHoliday> Adjustments { get; set; }
}
