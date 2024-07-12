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
    public class CategoryCategorySEOTemplateMappingService : ICategoryCategorySEOTemplateMappingService
    {
        private readonly IRepository<Category> _categoryRepository;
        #region Fields

        private readonly IRepository<CategoryCategorySEOTemplateMapping> _categoryCategorySEOTemplateMappingRepository;
        private readonly IRepository<CategorySEOTemplate> _categorySEOTemplateRepository;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        public CategoryCategorySEOTemplateMappingService(
            IRepository<Category> categoryRepository,
            IRepository<CategoryCategorySEOTemplateMapping> categoryCategorySEOTemplateMappingRepository,
            IRepository<CategorySEOTemplate> categorySEOTemplateRepository,
            IStoreMappingService storeMappingService
            )
        {
            _categoryRepository = categoryRepository;
            _categoryCategorySEOTemplateMappingRepository = categoryCategorySEOTemplateMappingRepository;
            _categorySEOTemplateRepository = categorySEOTemplateRepository;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods

        public async Task DeleteCategoryCategorySEOTemplateMappingAsync(CategoryCategorySEOTemplateMapping categoryCategorySEOTemplateMapping)
        {
            if (categoryCategorySEOTemplateMapping == null)
                throw new ArgumentNullException(nameof(categoryCategorySEOTemplateMapping));

            await _categoryCategorySEOTemplateMappingRepository.DeleteAsync(categoryCategorySEOTemplateMapping);
        }


        public async Task DeleteCategoryCategorySEOTemplateMappingsAsync(IList<CategoryCategorySEOTemplateMapping> categoryCategorySEOTemplateMappings)
        {
            if (categoryCategorySEOTemplateMappings == null)
                throw new ArgumentNullException(nameof(categoryCategorySEOTemplateMappings));

            await _categoryCategorySEOTemplateMappingRepository.DeleteAsync(categoryCategorySEOTemplateMappings);
        }

        public async Task<IPagedList<CategoryCategorySEOTemplateMapping>> GetAllCategoryCategorySEOTemplateMappingAsync(
            int categorySEOTemplateId = 0,
            int categoryId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = _categoryCategorySEOTemplateMappingRepository.Table;

            if(categoryId > 0)
                query = query.Where(ccm => ccm.CategoryId == categoryId);

            if(categorySEOTemplateId > 0)
                query = query.Where(ccm => ccm.CategorySEOTemplateId == categorySEOTemplateId);

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);

        }

        public async Task<IPagedList<CategorySEOTemplate>> GetAllMappedCategorySEOTemplateByCategoryIdAsync(
            int categoryId,
            int storeId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if(pageSize == int.MaxValue) 
                --pageSize;

            if (categoryId < 1)
                throw new ArgumentNullException(nameof(categoryId));

            var query = from ct in _categorySEOTemplateRepository.Table
                        join cctm in _categoryCategorySEOTemplateMappingRepository.Table on ct.Id equals cctm.CategorySEOTemplateId
                        where cctm.CategoryId == categoryId && !ct.IsGlobalTemplate && ct.IsActive && !ct.Deleted
                        select ct;

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            return await query.OrderBy(ct => ct.Id).OrderBy(ct => ct.Priority).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<CategoryCategorySEOTemplateMapping> GetCategoryCategorySEOTemplateMappingAsync(
            int categorySEOTemplateId,
            int categoryId
            )
        {
            if(categorySEOTemplateId < 1)
                throw new ArgumentNullException(nameof(categorySEOTemplateId));

            if(categoryId < 1)
                throw new ArgumentNullException(nameof(categoryId));

            var query = await _categoryCategorySEOTemplateMappingRepository.Table.FirstOrDefaultAsync(ccm => ccm.CategorySEOTemplateId == categorySEOTemplateId && ccm.CategoryId == categoryId);

            return query;
        }

        public async Task<IPagedList<Category>> GetAllMappedCategoriesByCategoryId(
            int categoryId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = from c in _categoryRepository.Table
                        join ccm in _categoryCategorySEOTemplateMappingRepository.Table on c.Id equals ccm.CategoryId
                        where c.Id == categoryId && !c.Deleted
                        select c;

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IPagedList<Category>> GetAllMappedCategoriesByCategorySEOTemplateId(
            int categorySEOTemplateId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = from c in _categoryRepository.Table
                        join ccm in _categoryCategorySEOTemplateMappingRepository.Table on c.Id equals ccm.CategoryId
                        where ccm.CategorySEOTemplateId == categorySEOTemplateId && !c.Deleted
                        select c;

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IPagedList<Category>> GetAllNotMappedCategoriesByCategorySEOTemplateId(
            int categorySEOTemplateId,
            string categoryName = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = from c in _categoryRepository.Table
                        where !c.Deleted && (categoryName == null || categoryName == string.Empty || c.Name.Contains(categoryName))
                        select c;

            var mappedCategoryIds = (from c in query
                                  select c.Id)
                                  .Except(from ccm in _categoryCategorySEOTemplateMappingRepository.Table
                                          where ccm.CategorySEOTemplateId == categorySEOTemplateId
                                          select ccm.CategoryId
                                          );
            query = from c in query
                    join mci in mappedCategoryIds on c.Id equals mci
                    select c;

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IPagedList<Category>> GetAllMappedCategoriesByMappingId(
            int categoryCategorySEOTemplateMappingId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = from c in _categoryRepository.Table
                        join ccm in _categoryCategorySEOTemplateMappingRepository.Table on c.Id equals ccm.CategoryId
                        where c.Id == categoryCategorySEOTemplateMappingId && !c.Deleted
                        select c;

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<CategoryCategorySEOTemplateMapping> GetCategoryCategorySEOTemplateMappingByIdAsync(int categoryCategorySEOTemplateMappingId, bool includeDeleted = false)
        {
            if (categoryCategorySEOTemplateMappingId < 1)
                throw new ArgumentNullException(nameof(categoryCategorySEOTemplateMappingId));

            return await _categoryCategorySEOTemplateMappingRepository.GetByIdAsync(categoryCategorySEOTemplateMappingId, includeDeleted: includeDeleted);
        }

        public async Task<IList<CategoryCategorySEOTemplateMapping>> GetCategoryCategorySEOTemplateMappingByIdsAsync(int[] categoryCategorySEOTemplateMappingIds, bool includeDeleted = false)
        {
            return await _categoryCategorySEOTemplateMappingRepository.GetByIdsAsync(categoryCategorySEOTemplateMappingIds, includeDeleted: includeDeleted);
        }

        public async Task InsertCategoryCategorySEOTemplateMappingAsync(CategoryCategorySEOTemplateMapping categoryCategorySEOTemplateMapping)
        {
            if (categoryCategorySEOTemplateMapping == null)
                throw new ArgumentNullException(nameof(categoryCategorySEOTemplateMapping));

            await _categoryCategorySEOTemplateMappingRepository.InsertAsync(categoryCategorySEOTemplateMapping);
        }

        public async Task UpdateCategoryCategorySEOTemplateMappingAsync(CategoryCategorySEOTemplateMapping categoryCategorySEOTemplateMapping)
        {
            if (categoryCategorySEOTemplateMapping == null)
                throw new ArgumentNullException(nameof(categoryCategorySEOTemplateMapping));

            await _categoryCategorySEOTemplateMappingRepository.UpdateAsync(categoryCategorySEOTemplateMapping);
        }

        #endregion
    }
}
