using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProductTabs.Domains;
using NopStation.Plugin.Widgets.ProductTabs.Models;

namespace NopStation.Plugin.Widgets.ProductTabs.Factories
{
    public interface IProductTabModelFactory
    {
        Task<IList<ProductTabModel>> PrepareProductTabListModelAsync(List<ProductTab> productTabs);
        Task<IList<ProductTabModel>> PrepareProductTabListModelAsync(string widgetZone);
    }
}
