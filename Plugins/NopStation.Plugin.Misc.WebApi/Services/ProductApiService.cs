using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Data;

namespace NopStation.Plugin.Misc.WebApi.Services
{
    public class ProductApiService : IProductApiService
    {
        private readonly IRepository<Product> _productRepository;
        public ProductApiService(IRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<Product> GetProductByGtinAsync(string gtin)
        {
            if (string.IsNullOrEmpty(gtin))
                return null;

            var query = from p in _productRepository.Table
                        orderby p.Id
                        where !p.Deleted &&
                        p.Gtin == gtin
                        select p;

            var product = await query.FirstOrDefaultAsync();
            return product;
        }
    }
}
