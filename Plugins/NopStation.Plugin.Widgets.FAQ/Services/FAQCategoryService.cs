using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Services
{
    public class FAQCategoryService : IFAQCategoryService
    {
        #region Fields

        private readonly IRepository<FAQCategory> _categoryRepository;
        private readonly IRepository<FAQItemCategory> _itemCategoryRepository;
        private readonly IRepository<FAQItem> _itemRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public FAQCategoryService(IRepository<FAQCategory> categoryRepository,
            IRepository<FAQItemCategory> itemCategoryRepository,
            IRepository<FAQItem> itemRepository,
            IStaticCacheManager staticCacheManager,
            IStoreMappingService storeMappingService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _categoryRepository = categoryRepository;
            _itemCategoryRepository = itemCategoryRepository;
            _itemRepository = itemRepository;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        protected virtual async Task<IList<FAQItemCategory>> GetFAQItemCategoriesByFAQItemIdAsync(int itemId, int storeId,
            bool showHidden = false)
        {
            if (itemId == 0)
                return new List<FAQItemCategory>();

            var customer = await _workContext.GetCurrentCustomerAsync();

            return await _itemCategoryRepository.GetAllAsync(async query =>
            {
                if (!showHidden)
                {
                    var categoriesQuery = _categoryRepository.Table.Where(c => c.Published);

                    //apply store mapping constraints
                    categoriesQuery = await _storeMappingService.ApplyStoreMapping(categoriesQuery, storeId);

                    query = query.Where(pc => categoriesQuery.Any(c => !c.Deleted && c.Id == pc.FAQCategoryId));
                }

                return query
                    .Where(pc => pc.FAQItemId == itemId)
                    .OrderBy(pc => pc.FAQCategoryId);
            });
        }

        #endregion

        #region Methods

        #region Categories

        public async Task InsertFAQCategoryAsync(FAQCategory category)
        {
            await _categoryRepository.InsertAsync(category);
        }

        public async Task UpdateFAQCategoryAsync(FAQCategory category)
        {
            await _categoryRepository.UpdateAsync(category);
        }

        public async Task DeleteFAQCategoryAsync(FAQCategory category)
        {
            await _categoryRepository.DeleteAsync(category);
        }

        public async Task<FAQCategory> GetFAQCategoryByIdAsync(int categoryId)
        {
            return await _categoryRepository.GetByIdAsync(categoryId, cache =>
                _staticCacheManager.PrepareKeyForDefaultCache(NopStationEntityCacheDefaults<FAQCategory>.ByIdCacheKey, categoryId));
        }

        public async Task<IPagedList<FAQCategory>> SearchFAQCategoriesAsync(string name = "", int storeId = 0,
            bool? published = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from c in _categoryRepository.Table
                        where !c.Deleted &&
                        (string.IsNullOrWhiteSpace(name) || c.Name.Contains(name)) &&
                        (!published.HasValue || c.Published == published.Value)
                        select c;

            if (storeId > 0)
            {
                query = await _storeMappingService.ApplyStoreMapping(query, storeId);
            }

            query = query.OrderBy(x => x.DisplayOrder);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion

        #region Item categqories

        public virtual async Task DeleteFAQItemCategoryAsync(FAQItemCategory categoryMapping)
        {
            await _itemCategoryRepository.DeleteAsync(categoryMapping);
        }

        public virtual async Task<IList<FAQItemCategory>> GetFAQItemCategoriesByItemIdAsync(int itemId, bool showHidden = false)
        {
            return await GetFAQItemCategoriesByFAQItemIdAsync(itemId, (await _storeContext.GetCurrentStoreAsync()).Id, showHidden);
        }

        public virtual FAQItemCategory FindFAQItemCategory(IList<FAQItemCategory> source, int itemId, int categoryId)
        {
            foreach (var itemCategory in source)
                if (itemCategory.FAQItemId == itemId && itemCategory.FAQCategoryId == categoryId)
                    return itemCategory;

            return null;
        }

        public virtual IList<int> GetFAQItemCategoryIds(FAQItem fAQItem)
        {
            if (fAQItem == null)
                throw new ArgumentNullException(nameof(fAQItem));

            var query = from q in _itemCategoryRepository.Table
                        where q.FAQItemId == fAQItem.Id
                        select q.FAQCategoryId;

            return query.ToList();
        }

        public virtual async Task<IPagedList<FAQItemCategory>> GetFAQItemCategoriesByCategoryIdAsync(int categoryId,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (categoryId == 0)
                return new PagedList<FAQItemCategory>(new List<FAQItemCategory>(), pageIndex, pageSize);

            var query = from ic in _itemCategoryRepository.Table
                        join p in _itemRepository.Table on ic.FAQItemId equals p.Id
                        where ic.FAQCategoryId == categoryId && !p.Deleted
                        orderby p.DisplayOrder
                        select ic;

            if (!showHidden)
            {
                var categoriesQuery = _categoryRepository.Table.Where(c => !c.Deleted && c.Published);

                //apply store mapping constraints
                var store = await _storeContext.GetCurrentStoreAsync();
                categoriesQuery = await _storeMappingService.ApplyStoreMapping(categoriesQuery, store.Id);

                query = query.Where(pc => categoriesQuery.Any(c => c.Id == pc.FAQCategoryId));
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task InsertFAQItemCategoryAsync(FAQItemCategory itemCategory)
        {
            await _itemCategoryRepository.InsertAsync(itemCategory);
        }

        #endregion

        #endregion
    }
}
