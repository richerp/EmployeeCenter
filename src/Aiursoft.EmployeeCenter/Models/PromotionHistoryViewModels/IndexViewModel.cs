using Aiursoft.EmployeeCenter.Entities;

namespace Aiursoft.EmployeeCenter.Models.PromotionHistoryViewModels;

public class IndexViewModel
{
    public int TotalPromotionsThisYear { get; set; }
    public int MyPromotionCount { get; set; }
    public TimeSpan TimeSinceLastPromotion { get; set; }
    public bool HasNeverBeenPromoted { get; set; }
    
    public IEnumerable<PromotionHistory> Histories { get; set; } = new List<PromotionHistory>();
}
