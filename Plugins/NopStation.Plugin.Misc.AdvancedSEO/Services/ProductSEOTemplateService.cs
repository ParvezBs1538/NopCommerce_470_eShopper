using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Services
{
    public class ProductSEOTemplateService : IProductSEOTemplateService
    {
        #region Fields

        private readonly IRepository<ProductSEOTemplate> _productSEOTemplateRepository;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        public ProductSEOTemplateService(
            IRepository<ProductSEOTemplate> productSEOTemplateRepository,
            IStoreMappingService storeMappingService
            )
        {
            _productSEOTemplateRepository = productSEOTemplateRepository;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods

        public async Task DeleteProductSEOTemplateAsync(ProductSEOTemplate productSEOTemplate)
        {
            if (productSEOTemplate == null)
                throw new ArgumentNullException(nameof(productSEOTemplate));

            await _productSEOTemplateRepository.DeleteAsync(productSEOTemplate);
        }


        public async Task DeletePosInvoicesTypeAsync(IList<ProductSEOTemplate> productSEOTemplates)
        {
            if (productSEOTemplates == null)
                throw new ArgumentNullException(nameof(productSEOTemplates));

            await _productSEOTemplateRepository.DeleteAsync(productSEOTemplates);
        }

        public async Task<IPagedList<ProductSEOTemplate>> GetAllProductSEOTemplateAsync(
            string name = null,
            int storeId = 0,
            bool? isGlobalTemplate = null,
            bool? isActive = null,
            bool includeDeleted = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = _productSEOTemplateRepository.Table;


            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            if (name != null && !string.IsNullOrEmpty(name))
                query = query.Where(cst => cst.TemplateName.Contains(name));

            if (isActive.HasValue)
                query = query.Where(cst => cst.IsActive == isActive.Value);

            if (isGlobalTemplate.HasValue)
                query = query.Where(cst => cst.IsGlobalTemplate == isGlobalTemplate.Value);

            if (!includeDeleted)
                query = query.Where(cst => cst.Deleted == false);


            return await query.OrderBy(cst => cst.Id).OrderBy(cst => cst.Priority).ToPagedListAsync(pageIndex, pageSize);

        }

        public async Task<ProductSEOTemplate> GetProductSEOTemplateByIdAsync(int productSEOTemplateId, bool includeDeleted = false)
        {
            if (productSEOTemplateId < 1)
                throw new ArgumentNullException(nameof(productSEOTemplateId));

            return await _productSEOTemplateRepository.GetByIdAsync(productSEOTemplateId, includeDeleted: includeDeleted);
        }

        public async Task<IList<ProductSEOTemplate>> GetProductSEOTemplateByIdsAsync(int[] productSEOTemplateIds, bool includeDeleted = false)
        {
            return await _productSEOTemplateRepository.GetByIdsAsync(productSEOTemplateIds, includeDeleted: includeDeleted);
        }

        public async Task InsertProductSEOTemplateAsync(ProductSEOTemplate productSEOTemplate)
        {
            if (productSEOTemplate == null)
                throw new ArgumentNullException(nameof(productSEOTemplate));

            await _productSEOTemplateRepository.InsertAsync(productSEOTemplate);
        }

        public async Task UpdateProductSEOTemplateAsync(ProductSEOTemplate productSEOTemplate)
        {
            if (productSEOTemplate == null)
                throw new ArgumentNullException(nameof(productSEOTemplate));

            await _productSEOTemplateRepository.UpdateAsync(productSEOTemplate);
        }

        #endregion
    }
}
