using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.ProductRequests.Domains;

namespace NopStation.Plugin.Widgets.ProductRequests.Services
{
    public interface IProductRequestService
    {
        Task<IPagedList<ProductRequest>> GetAllProductRequestsAsync(string name = "", int customerId = 0,
            string customerEmail = "", int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<ProductRequest> GetProductRequestByIdAsync(int productRequestId);

        Task InsertProductRequestAsync(ProductRequest productRequest);

        Task UpdateProductRequestAsync(ProductRequest productRequest);

        Task DeleteProductRequestAsync(ProductRequest productRequest);
    }
}