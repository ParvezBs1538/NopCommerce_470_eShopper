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
    public interface ICategoryCategorySEOTemplateMappingService
    {
        Task DeleteCategoryCategorySEOTemplateMappingAsync(CategoryCategorySEOTemplateMapping categoryCategorySEOTemplateMapping);

        Task DeleteCategoryCategorySEOTemplateMappingsAsync(IList<CategoryCategorySEOTemplateMapping> categoryCategorySEOTemplateMappings);

        Task<IPagedList<CategoryCategorySEOTemplateMapping>> GetAllCategoryCategorySEOTemplateMappingAsync(
            int categorySEOTemplateId = 0,
            int categoryId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<IPagedList<CategorySEOTemplate>> GetAllMappedCategorySEOTemplateByCategoryIdAsync(
            int categoryId,
            int storeId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<CategoryCategorySEOTemplateMapping> GetCategoryCategorySEOTemplateMappingAsync(
            int categorySEOTemplateId,
            int categoryId
            );

        Task<CategoryCategorySEOTemplateMapping> GetCategoryCategorySEOTemplateMappingByIdAsync(int categoryCategorySEOTemplateMappingId, bool includeDeleted = false);

        Task<IList<CategoryCategorySEOTemplateMapping>> GetCategoryCategorySEOTemplateMappingByIdsAsync(int[] categoryCategorySEOTemplateMappingIds, bool includeDeleted = false);

        Task InsertCategoryCategorySEOTemplateMappingAsync(CategoryCategorySEOTemplateMapping categoryCategorySEOTemplateMapping);

        Task UpdateCategoryCategorySEOTemplateMappingAsync(CategoryCategorySEOTemplateMapping categoryCategorySEOTemplateMapping);

        Task<IPagedList<Category>> GetAllMappedCategoriesByMappingId(
            int categorySEOTemplateId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<IPagedList<Category>> GetAllMappedCategoriesByCategoryId(
            int categoryId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<IPagedList<Category>> GetAllNotMappedCategoriesByCategorySEOTemplateId(
            int categorySEOTemplateId,
            string categoryName = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

    }
}
