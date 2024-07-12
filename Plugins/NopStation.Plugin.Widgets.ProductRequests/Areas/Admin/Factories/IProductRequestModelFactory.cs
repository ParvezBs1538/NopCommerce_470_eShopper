using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductRequests.Domains;

namespace NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Factories
{
    public interface IProductRequestModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();

        Task<ProductRequestSearchModel> PrepareProductRequestSearchModelAsync(ProductRequestSearchModel searchModel);

        Task<ProductRequestListModel> PrepareProductRequestListModelAsync(ProductRequestSearchModel searchModel);

        Task<ProductRequestModel> PrepareProductRequestModelAsync(ProductRequestModel model, ProductRequest productRequest, 
            bool excludeProperties = false);
    }
}