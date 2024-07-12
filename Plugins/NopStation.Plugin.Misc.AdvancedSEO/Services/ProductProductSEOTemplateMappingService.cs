using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2013.Word;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Services
{
    public class ProductProductSEOTemplateMappingService : IProductProductSEOTemplateMappingService
    {
        private readonly IRepository<Product> _productRepository;
        #region Fields

        private readonly IRepository<ProductProductSEOTemplateMapping> _productProductSEOTemplateMappingRepository;
        private readonly IRepository<ProductSEOTemplate> _productSEOTemplateRepository;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        public ProductProductSEOTemplateMappingService(
            IRepository<Product> productRepository,
            IRepository<ProductProductSEOTemplateMapping> productProductSEOTemplateMappingRepository,
            IRepository<ProductSEOTemplate> productSEOTemplateRepository,
            IStoreMappingService storeMappingService
            )
        {
            _productRepository = productRepository;
            _productProductSEOTemplateMappingRepository = productProductSEOTemplateMappingRepository;
            _productSEOTemplateRepository = productSEOTemplateRepository;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods

        public async Task DeleteProductProductSEOTemplateMappingAsync(ProductProductSEOTemplateMapping productProductSEOTemplateMapping)
        {
            if (productProductSEOTemplateMapping == null)
                throw new ArgumentNullException(nameof(productProductSEOTemplateMapping));

            await _productProductSEOTemplateMappingRepository.DeleteAsync(productProductSEOTemplateMapping);
        }


        public async Task DeleteProductProductSEOTemplateMappingsAsync(IList<ProductProductSEOTemplateMapping> productProductSEOTemplateMappings)
        {
            if (productProductSEOTemplateMappings == null)
                throw new ArgumentNullException(nameof(productProductSEOTemplateMappings));

            await _productProductSEOTemplateMappingRepository.DeleteAsync(productProductSEOTemplateMappings);
        }

        public async Task<IPagedList<ProductProductSEOTemplateMapping>> GetAllProductProductSEOTemplateMappingAsync(
            int productSEOTemplateId = 0,
            int productId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = _productProductSEOTemplateMappingRepository.Table;

            if(productId > 0)
                query = query.Where(ccm => ccm.ProductId == productId);

            if(productSEOTemplateId > 0)
                query = query.Where(ccm => ccm.ProductSEOTemplateId == productSEOTemplateId);

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);

        }

        public async Task<IPagedList<ProductSEOTemplate>> GetAllMappedProductSEOTemplateByProductIdAsync(
            int productId,
            int storeId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if(pageSize == int.MaxValue) 
                --pageSize;

            if (productId < 1)
                throw new ArgumentNullException(nameof(productId));

            var query = from pt in _productSEOTemplateRepository.Table
                        join pptm in _productProductSEOTemplateMappingRepository.Table on pt.Id equals pptm.ProductSEOTemplateId
                        where pptm.ProductId == productId && !pt.IsGlobalTemplate && pt.IsActive && !pt.Deleted
                        select pt;

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            return await query.OrderBy(ct => ct.Id).OrderBy(ct => ct.Priority).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<ProductProductSEOTemplateMapping> GetProductProductSEOTemplateMappingAsync(
            int productSEOTemplateId,
            int productId
            )
        {
            if(productSEOTemplateId < 1)
                throw new ArgumentNullException(nameof(productSEOTemplateId));

            if(productId < 1)
                throw new ArgumentNullException(nameof(productId));

            var query = await _productProductSEOTemplateMappingRepository.Table.FirstOrDefaultAsync(ccm => ccm.ProductSEOTemplateId == productSEOTemplateId && ccm.ProductId == productId);

            return query;
        }

        public async Task<IPagedList<Product>> GetAllMappedCategoriesByProductId(
            int productId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = from c in _productRepository.Table
                        join ccm in _productProductSEOTemplateMappingRepository.Table on c.Id equals ccm.ProductId
                        where c.Id == productId && !c.Deleted
                        select c;

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IPagedList<Product>> GetAllMappedCategoriesByProductSEOTemplateId(
            int productSEOTemplateId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = from c in _productRepository.Table
                        join ccm in _productProductSEOTemplateMappingRepository.Table on c.Id equals ccm.ProductId
                        where ccm.ProductSEOTemplateId == productSEOTemplateId && !c.Deleted
                        select c;

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IPagedList<Product>> GetAllNotMappedCategoriesByProductSEOTemplateId(
            int productSEOTemplateId,
            string productName = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = from c in _productRepository.Table
                        where !c.Deleted && (productName == null || productName == string.Empty || c.Name.Contains(productName))
                        select c;

            var mappedProductIds = (from c in query
                                  select c.Id)
                                  .Except(from ccm in _productProductSEOTemplateMappingRepository.Table
                                          where ccm.ProductSEOTemplateId == productSEOTemplateId
                                          select ccm.ProductId
                                          );
            query = from c in query
                    join mci in mappedProductIds on c.Id equals mci
                    select c;

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IPagedList<Product>> GetAllMappedCategoriesByMappingId(
            int productProductSEOTemplateMappingId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = from c in _productRepository.Table
                        join ccm in _productProductSEOTemplateMappingRepository.Table on c.Id equals ccm.ProductId
                        where c.Id == productProductSEOTemplateMappingId && !c.Deleted
                        select c;

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<ProductProductSEOTemplateMapping> GetProductProductSEOTemplateMappingByIdAsync(int productProductSEOTemplateMappingId, bool includeDeleted = false)
        {
            if (productProductSEOTemplateMappingId < 1)
                throw new ArgumentNullException(nameof(productProductSEOTemplateMappingId));

            return await _productProductSEOTemplateMappingRepository.GetByIdAsync(productProductSEOTemplateMappingId, includeDeleted: includeDeleted);
        }

        public async Task<IList<ProductProductSEOTemplateMapping>> GetProductProductSEOTemplateMappingByIdsAsync(int[] productProductSEOTemplateMappingIds, bool includeDeleted = false)
        {
            return await _productProductSEOTemplateMappingRepository.GetByIdsAsync(productProductSEOTemplateMappingIds, includeDeleted: includeDeleted);
        }

        public async Task InsertProductProductSEOTemplateMappingAsync(ProductProductSEOTemplateMapping productProductSEOTemplateMapping)
        {
            if (productProductSEOTemplateMapping == null)
                throw new ArgumentNullException(nameof(productProductSEOTemplateMapping));

            await _productProductSEOTemplateMappingRepository.InsertAsync(productProductSEOTemplateMapping);
        }

        public async Task UpdateProductProductSEOTemplateMappingAsync(ProductProductSEOTemplateMapping productProductSEOTemplateMapping)
        {
            if (productProductSEOTemplateMapping == null)
                throw new ArgumentNullException(nameof(productProductSEOTemplateMapping));

            await _productProductSEOTemplateMappingRepository.UpdateAsync(productProductSEOTemplateMapping);
        }

        #endregion
    }
}
