using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.PromotionHistoryViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel()
    {
        PageTitle = "Promotion History";
    }

    public int TotalPromotionsThisYear { get; set; }
    public int MyPromotionCount { get; set; }
    public TimeSpan TimeSinceLastPromotion { get; set; }
    public bool HasNeverBeenPromoted { get; set; }
    
    public IEnumerable<PromotionHistory> Histories { get; set; } = new List<PromotionHistory>();
}
