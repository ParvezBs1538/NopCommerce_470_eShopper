using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.Widgets.GoogleAnalytics.Services
{
    public interface INSGoogleAnalyticsService
    {
        public Task<string> PostAsync<T>(T eventParam, string eventName);
        public Task<ProductDetailsModel.ProductPriceModel> PrepareProductPriceModelAsync(Product product);
    }
}
