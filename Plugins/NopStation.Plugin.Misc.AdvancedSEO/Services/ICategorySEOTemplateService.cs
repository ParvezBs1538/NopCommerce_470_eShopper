using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Services
{
    public interface ICategorySEOTemplateService
    {
        Task DeleteCategorySEOTemplateAsync(CategorySEOTemplate categorySEOTemplate);

        Task DeleteCategorySEOTemplatesAsync(IList<CategorySEOTemplate> categorySEOTemplates);

        Task<IPagedList<CategorySEOTemplate>> GetAllCategorySEOTemplateAsync(
            string name = null,
            int storeId = 0,
            bool? isGlobalTemplate = null,
            bool? isActive = null,
            bool includeDeleted = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<CategorySEOTemplate> GetCategorySEOTemplateByIdAsync(int categorySEOTemplateId, bool includeDeleted = false);

        Task<IList<CategorySEOTemplate>> GetCategorySEOTemplateByIdsAsync(int[] categorySEOTemplateIds, bool includeDeleted = false);

        Task InsertCategorySEOTemplateAsync(CategorySEOTemplate categorySEOTemplate);

        Task UpdateCategorySEOTemplateAsync(CategorySEOTemplate categorySEOTemplate);
    }
}
