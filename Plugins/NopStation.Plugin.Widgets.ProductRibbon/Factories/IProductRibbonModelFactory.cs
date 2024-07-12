using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Widgets.ProductRibbon.Models;

namespace NopStation.Plugin.Widgets.ProductRibbon.Factories
{
    public interface IProductRibbonModelFactory
    {
        Task<ProductRibbonModel> PrepareProductRibbonModelAsync(Product product);
    }
}