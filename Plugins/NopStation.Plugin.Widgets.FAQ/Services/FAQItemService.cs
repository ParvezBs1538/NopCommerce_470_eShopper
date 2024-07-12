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
    public class FAQItemService : IFAQItemService
    {
        #region Fields

        private readonly IRepository<FAQItem> _itemRepository;
        private readonly IRepository<FAQItemTag> _itemTagMappingRepository;
        private readonly IRepository<FAQItemCategory> _itemCategoryMappingRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        public FAQItemService(IRepository<FAQItem> itemRepository,
            IRepository<FAQItemTag> itemTagMappingRepository,
            IRepository<FAQItemCategory> itemCategoryMappingRepository,
            IStaticCacheManager staticCacheManager,
            IStoreMappingService storeMappingService)
        {
            _itemRepository = itemRepository;
            _itemTagMappingRepository = itemTagMappingRepository;
            _itemCategoryMappingRepository = itemCategoryMappingRepository;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Methods

        public async Task InsertFAQItemAsync(FAQItem item)
        {
            await _itemRepository.InsertAsync(item);
        }

        public async Task UpdateFAQItemAsync(FAQItem item)
        {
            await _itemRepository.UpdateAsync(item);
        }

        public async Task DeleteFAQItemAsync(FAQItem item)
        {
            await _itemRepository.DeleteAsync(item);
        }

        public async Task<FAQItem> GetFAQItemByIdAsync(int itemId)
        {
            return await _itemRepository.GetByIdAsync(itemId, cache =>
                _staticCacheManager.PrepareKeyForDefaultCache(NopStationEntityCacheDefaults<FAQItem>.ByIdCacheKey, itemId));
        }

        public async Task<IPagedList<FAQItem>> SearchFAQItemsAsync(string keyword = "", int categoryId = 0, int tagId = 0,
            bool? published = null, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from i in _itemRepository.Table
                        where !i.Deleted &&
                        (string.IsNullOrWhiteSpace(keyword) || i.Question.Contains(keyword) || i.Answer.Contains(keyword)) &&
                        (!published.HasValue || i.Published == published.Value)
                        select i;

            if (tagId > 0)
            {
                query = from i in query
                        join it in _itemTagMappingRepository.Table on i.Id equals it.FAQItemId
                        where it.FAQTagId == tagId
                        select i;
            }

            if (categoryId > 0)
            {
                query = from i in query
                        join ic in _itemCategoryMappingRepository.Table on i.Id equals ic.FAQItemId
                        where ic.FAQCategoryId == categoryId
                        select i;
            }

            if (storeId > 0)
            {
                query = await _storeMappingService.ApplyStoreMapping(query, storeId);
            }

            query = query.OrderByDescending(x => x.DisplayOrder);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
