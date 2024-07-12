using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Customers;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Domains;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Infrastructure.Cache;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Services
{
    public class ProductQAService : IProductQAService
    {
        #region Fields
        private readonly IRepository<ProductQnA> _productQARepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IStaticCacheManager _cacheManager;
        #endregion

        #region Ctor
        public ProductQAService(
            IRepository<ProductQnA> productQARepository,
            IRepository<Product> productRepository,
            IWorkContext workContext,
            ICustomerService customerService,
            IStaticCacheManager cacheManager)
        {
            _productQARepository = productQARepository;
            _productRepository = productRepository;
            _workContext = workContext;
            _customerService = customerService;
            _cacheManager = cacheManager;
        }
        #endregion

        #region Methods

        public async Task DeleteProductQnAAsync(ProductQnA productQA)
        {
            await _productQARepository.DeleteAsync(productQA);
        }

        public async Task InsertProductQnAAsync(ProductQnA productQA)
        {
            await _productQARepository.InsertAsync(productQA);
        }

        public async Task UpdateProductQnAAsync(ProductQnA productQA)
        {
            await _productQARepository.UpdateAsync(productQA);
        }

        public async Task<ProductQnA> GetProductQnAByIdAsync(int productQAId)
        {
            return await _productQARepository.GetByIdAsync(productQAId);
        }

        public async Task<IPagedList<ProductQnA>> GetAllProductQnAsAsync(int storeId = 0, int productId = 0, 
            bool? approved = null, bool? hasAnswer = null, DateTime? createdFrom = null, DateTime? createdTo = null, 
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var vendorId = 0;
            if (await _customerService.IsVendorAsync(await _workContext.GetCurrentCustomerAsync()))
                vendorId= (await _workContext.GetCurrentVendorAsync()).Id;

            var query = from qna in _productQARepository.Table
                        join p in _productRepository.Table on qna.ProductId equals p.Id
                        where !qna.Deleted && !p.Deleted &&
                        (vendorId == 0 || p.VendorId == vendorId) &&
                        (storeId == 0 || qna.StoreId == storeId) &&
                        (productId == 0 || qna.ProductId == productId) &&
                        (!approved.HasValue || qna.IsApproved == approved.Value) &&
                        (!hasAnswer.HasValue || !string.IsNullOrWhiteSpace(qna.AnswerText)) &&
                        (!createdFrom.HasValue || qna.CreatedOnUtc >= createdFrom.Value) &&
                        (!createdTo.HasValue || qna.CreatedOnUtc <= createdTo.Value)
                        orderby qna.CreatedOnUtc descending
                        select qna;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IList<ProductQnA>> GetProductQnAsByProductIdAsync(int productId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var currentCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;

            var key = _cacheManager.PrepareKeyForDefaultCache(ProductQACache.ProductQAAllByProductIdCacheKey, productId, currentCustomerId, pageIndex, pageSize);

            return await _cacheManager.GetAsync(key, async () =>
            {
                var query = from qna in _productQARepository.Table
                            join p in _productRepository.Table on qna.ProductId equals p.Id
                            where !qna.Deleted && !p.Deleted &&
                            (qna.CustomerId == currentCustomerId || qna.IsApproved)&&
                            (qna.ProductId == productId)
                            orderby qna.CreatedOnUtc descending
                            select qna;
                var result = await query.ToListAsync();
                
                return result;
            });
        }

        #endregion
    }
}
