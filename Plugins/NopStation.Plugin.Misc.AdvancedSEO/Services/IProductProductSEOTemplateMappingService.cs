using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Services
{
    public interface IProductProductSEOTemplateMappingService
    {
        Task DeleteProductProductSEOTemplateMappingAsync(ProductProductSEOTemplateMapping productProductSEOTemplateMapping);

        Task DeleteProductProductSEOTemplateMappingsAsync(IList<ProductProductSEOTemplateMapping> productProductSEOTemplateMappings);

        Task<IPagedList<ProductProductSEOTemplateMapping>> GetAllProductProductSEOTemplateMappingAsync(
            int productSEOTemplateId = 0,
            int productId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<IPagedList<ProductSEOTemplate>> GetAllMappedProductSEOTemplateByProductIdAsync(
            int productId,
            int storeId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<ProductProductSEOTemplateMapping> GetProductProductSEOTemplateMappingAsync(
            int productSEOTemplateId,
            int productId
            );

        Task<ProductProductSEOTemplateMapping> GetProductProductSEOTemplateMappingByIdAsync(int productProductSEOTemplateMappingId, bool includeDeleted = false);

        Task<IList<ProductProductSEOTemplateMapping>> GetProductProductSEOTemplateMappingByIdsAsync(int[] productProductSEOTemplateMappingIds, bool includeDeleted = false);

        Task InsertProductProductSEOTemplateMappingAsync(ProductProductSEOTemplateMapping productProductSEOTemplateMapping);

        Task UpdateProductProductSEOTemplateMappingAsync(ProductProductSEOTemplateMapping productProductSEOTemplateMapping);

        Task<IPagedList<Product>> GetAllMappedCategoriesByMappingId(
            int productSEOTemplateId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<IPagedList<Product>> GetAllMappedCategoriesByProductId(
            int productId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<IPagedList<Product>> GetAllNotMappedCategoriesByProductSEOTemplateId(
            int productSEOTemplateId,
            string productName = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

    }
}
