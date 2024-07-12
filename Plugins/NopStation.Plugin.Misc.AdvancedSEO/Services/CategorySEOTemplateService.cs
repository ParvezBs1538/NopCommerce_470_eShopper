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
    public class CategorySEOTemplateService : ICategorySEOTemplateService
    {
        #region Fields

        private readonly IRepository<CategorySEOTemplate> _categorySEOTemplateRepository;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        public CategorySEOTemplateService(
            IRepository<CategorySEOTemplate> categorySEOTemplateRepository,
            IStoreMappingService storeMappingService
            )
        {
            _categorySEOTemplateRepository = categorySEOTemplateRepository;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods

        public async Task DeleteCategorySEOTemplateAsync(CategorySEOTemplate categorySEOTemplate)
        {
            if(categorySEOTemplate == null)
                throw new ArgumentNullException(nameof(categorySEOTemplate));

            await _categorySEOTemplateRepository.DeleteAsync(categorySEOTemplate);
        }
        

        public async Task DeleteCategorySEOTemplatesAsync(IList<CategorySEOTemplate> categorySEOTemplates)
        {
            if(categorySEOTemplates == null)
                throw new ArgumentNullException(nameof(categorySEOTemplates));

            await _categorySEOTemplateRepository.DeleteAsync(categorySEOTemplates);
        }

        public async Task<IPagedList<CategorySEOTemplate>> GetAllCategorySEOTemplateAsync(
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
                -- pageSize;

            var query = _categorySEOTemplateRepository.Table;


            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            if(name != null && !string.IsNullOrEmpty(name))
                query = query.Where(cst => cst.TemplateName.Contains(name));

            if(isActive.HasValue)
                query = query.Where(cst => cst.IsActive == isActive.Value);

            if(isGlobalTemplate.HasValue)
                query = query.Where(cst => cst.IsGlobalTemplate == isGlobalTemplate.Value);
            
            if(!includeDeleted)
                query = query.Where(cst => cst.Deleted == false);


            return await query.OrderBy(cst => cst.Id).OrderBy(cst => cst.Priority).ToPagedListAsync(pageIndex, pageSize);

        }

        public async Task<CategorySEOTemplate> GetCategorySEOTemplateByIdAsync(int categorySEOTemplateId, bool includeDeleted = false)
        {
            if(categorySEOTemplateId < 1)
                throw new ArgumentNullException(nameof(categorySEOTemplateId));

            return await _categorySEOTemplateRepository.GetByIdAsync(categorySEOTemplateId, includeDeleted: includeDeleted);
        }

        public async Task<IList<CategorySEOTemplate>> GetCategorySEOTemplateByIdsAsync(int[] categorySEOTemplateIds, bool includeDeleted = false)
        {
            return await _categorySEOTemplateRepository.GetByIdsAsync(categorySEOTemplateIds, includeDeleted: includeDeleted);
        }

        public async Task InsertCategorySEOTemplateAsync(CategorySEOTemplate categorySEOTemplate)
        {
            if(categorySEOTemplate == null)
                throw new ArgumentNullException(nameof(categorySEOTemplate));

            await _categorySEOTemplateRepository.InsertAsync(categorySEOTemplate);
        }

        public async Task UpdateCategorySEOTemplateAsync(CategorySEOTemplate categorySEOTemplate)
        {
            if(categorySEOTemplate == null)
                throw new ArgumentNullException(nameof(categorySEOTemplate));

            await _categorySEOTemplateRepository.UpdateAsync(categorySEOTemplate);
        }

        #endregion
    }
}
