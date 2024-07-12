using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Services
{
    public interface IAlgoliaCatalogService
    {
        IList<Manufacturer> GetManufacturersByIds(int[] ids);

        Task<IPagedList<Product>> SearchProductsAsync(int fromId = 0, int toId = int.MaxValue, IList<int> productIds = null,
            IList<int> categoryIds = null, IList<int> manufacturerIds = null, IList<int> vendorIds = null,
            bool inProductIdsOnly = false, int pageIndex = 0, int pageSize = int.MaxValue);

        IList<ProductAttributeValue> GetAttributeValuesByIds(int[] ids);
        Task<IPagedList<Product>> GetProductsByEntityIdsAsync(IList<int> productIds = null,
           IList<int> categoryIds = null, IList<int> manufacturerIds = null, IList<int> vendorIds = null, int pageIndex = 0, int pageSize = int.MaxValue, bool deletedOrUnpublishProduct = false);
    }
}
