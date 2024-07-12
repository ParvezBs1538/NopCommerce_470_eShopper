using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace NopStation.Plugin.Widgets.PrevNextProduct.Services;

public interface IPrevNextProductService
{
    Task<(Product Previous, Product Next)> GetProductsAsync(int productId);
}