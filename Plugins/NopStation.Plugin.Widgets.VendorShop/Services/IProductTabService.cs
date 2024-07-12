using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Services
{
    public interface IProductTabService
    {
        Task DeleteProductTabAsync(ProductTab productTab);

        Task InsertProductTabAsync(ProductTab productTab);

        Task UpdateProductTabAsync(ProductTab productTab);

        Task<ProductTab> GetProductTabByIdAsync(int productTabId);

        Task<IPagedList<ProductTab>> GetAllProductTabsAsync(int vendorId, List<int> widgetZoneIds = null, bool hasItemsOnly = false,
            int storeId = 0, bool? active = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task DeleteProductTabItemAsync(ProductTabItem productTabItem);

        Task UpdateProductTabItemAsync(ProductTabItem productTabItem);

        Task InsertProductTabItemProductAsync(ProductTabItemProduct productTabItemProduct);
        Task DeleteProductTabItemProductAsync(ProductTabItemProduct productTabItemProduct);

        Task UpdateProductTabItemProductAsync(ProductTabItemProduct productTabItemProduct);

        Task InsertProductTabItemAsync(ProductTabItem productTabItem);

        Task<ProductTabItem> GetProductTabItemByIdAsync(int productTabItemId);

        Task<ProductTabItemProduct> GetProductTabItemProductByIdAsync(int productTabItemProductId);
        List<ProductTabItem> GetProductTabItemsByProductTabId(int productTabId);
        List<ProductTabItemProduct> GetProductTabItemProductsByProductTabItemId(int productTabItemId);
    }
}