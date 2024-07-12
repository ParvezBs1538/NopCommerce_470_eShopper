using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using NopStation.Plugin.Widgets.ProductRequests.Domains;

namespace NopStation.Plugin.Widgets.ProductRequests.Services
{
    public class ProductRequestService : IProductRequestService
    {
        #region Fields

        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<ProductRequest> _productRequestRepository;
        
        #endregion

        #region Ctor

        public ProductRequestService(IRepository<Customer> customerRepository,
            IRepository<ProductRequest> productRequestRepository)
        {
            _customerRepository = customerRepository;
            _productRequestRepository = productRequestRepository;
        }

        #endregion

        #region Methods

        public virtual async Task<IPagedList<ProductRequest>> GetAllProductRequestsAsync(string name = "", int customerId = 0, 
            string customerEmail = "", int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue) 
        {
            var query = from pr in _productRequestRepository.Table
                        join c in _customerRepository.Table on pr.CustomerId equals c.Id
                        where !c.Deleted && !pr.Deleted && (storeId == 0 || pr.StoreId == storeId) &&
                        (customerId == 0 || pr.CustomerId == customerId) &&
                        (string.IsNullOrWhiteSpace(customerEmail) || c.Email.Contains(customerEmail)) &&
                        (string.IsNullOrWhiteSpace(name) || pr.Name.Contains(name))
                        orderby pr.CreatedOnUtc descending
                        select pr;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async virtual Task<ProductRequest>  GetProductRequestByIdAsync(int productRequestId)
        {
            if (productRequestId == 0) 
                return null;

            return await _productRequestRepository.GetByIdAsync(productRequestId, cache => default);
        }

        public async virtual Task InsertProductRequestAsync(ProductRequest productRequest)
        {
            await _productRequestRepository.InsertAsync(productRequest);
        }

        public async virtual Task UpdateProductRequestAsync(ProductRequest productRequest)
        {
            await _productRequestRepository.UpdateAsync(productRequest);
        }

        public async virtual Task DeleteProductRequestAsync(ProductRequest productRequest)
        {
            await _productRequestRepository.DeleteAsync(productRequest);
        }

        #endregion
    }
}
