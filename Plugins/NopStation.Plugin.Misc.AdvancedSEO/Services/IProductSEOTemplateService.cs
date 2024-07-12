using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Services
{
    public interface IProductSEOTemplateService
    {
        Task DeleteProductSEOTemplateAsync(ProductSEOTemplate productSEOTemplate);

        Task DeletePosInvoicesTypeAsync(IList<ProductSEOTemplate> productSEOTemplates);

        Task<IPagedList<ProductSEOTemplate>> GetAllProductSEOTemplateAsync(
            string name = null,
            int storeId = 0,
            bool? isGlobalTemplate = null,
            bool? isActive = null,
            bool includeDeleted = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<ProductSEOTemplate> GetProductSEOTemplateByIdAsync(int productSEOTemplateId, bool includeDeleted = false);

        Task<IList<ProductSEOTemplate>> GetProductSEOTemplateByIdsAsync(int[] productSEOTemplateIds, bool includeDeleted = false);

        Task InsertProductSEOTemplateAsync(ProductSEOTemplate productSEOTemplate);

        Task UpdateProductSEOTemplateAsync(ProductSEOTemplate productSEOTemplate);

    }
}
