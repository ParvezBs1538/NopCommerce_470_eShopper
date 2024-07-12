using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.ProductRequests.Domains;
using NopStation.Plugin.Widgets.ProductRequests.Models;

namespace NopStation.Plugin.Widgets.ProductRequests.Factories
{
    public interface IProductRequestModelFactory
    {
        Task<ProductRequestListModel> PrepareProductRequestListModelAsync();

        ProductRequestModel PrepareAddNewProductRequestModel();

        Task<ProductRequestModel> PrepareProductRequestModelAsync(ProductRequest productRequest);
    }
}
