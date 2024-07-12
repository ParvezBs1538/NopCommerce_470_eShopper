using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace NopStation.Plugin.Misc.WebApi.Services
{
    public interface IProductApiService
    {
        Task<Product> GetProductByGtinAsync(string gtin);
    }
}
