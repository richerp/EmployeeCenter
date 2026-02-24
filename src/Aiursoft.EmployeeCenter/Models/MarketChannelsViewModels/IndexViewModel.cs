using Aiursoft.EmployeeCenter.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.EmployeeCenter.Models.MarketChannelsViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IEnumerable<MarketChannel> MarketChannels { get; init; } = [];
}
