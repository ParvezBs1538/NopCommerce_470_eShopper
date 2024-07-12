using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Models.ProductTabs;

namespace NopStation.Plugin.Widgets.VendorShop.Factories
{
    public interface IProductTabModelFactory
    {
        Task<IList<ProductTabModel>> PrepareProductTabListModelAsync(List<ProductTab> productTabs);
        Task<IList<ProductTabModel>> PrepareProductTabListModelAsync(int vendorId, string widgetZone);
    }
}
