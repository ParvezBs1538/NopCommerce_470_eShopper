using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Services
{
    public class FAQTagService : IFAQTagService
    {
        #region Fields

        private readonly IRepository<FAQTag> _tagRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IRepository<FAQItemTag> _itemTagMappingRepository;

        #endregion

        #region Ctor

        public FAQTagService(IRepository<FAQTag> tagRepository,
            IStaticCacheManager staticCacheManager,
            IRepository<FAQItemTag> itemTagMappingRepository)
        {
            _tagRepository = tagRepository;
            _staticCacheManager = staticCacheManager;
            _itemTagMappingRepository = itemTagMappingRepository;
        }

        #endregion

        #region Utilities

        protected virtual async Task<bool> FAQItemTagExistsAsync(FAQItem item, int tagId)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return await _itemTagMappingRepository.Table
                .AnyAsync(pptm => pptm.FAQItemId == item.Id && pptm.FAQTagId == tagId);
        }

        protected virtual async Task<FAQTag> GetFAQTagByNameAsync(string name)
        {
            var query = from pt in _tagRepository.Table
                        where pt.Name == name
                        select pt;

            var productTag = await query.FirstOrDefaultAsync();
            return productTag;
        }

        protected virtual async Task DeleteFAQItemTagMappingAsync(int itemId, int tagId)
        {
            var mappingRecord = await _itemTagMappingRepository.Table
                .FirstOrDefaultAsync(itm => itm.FAQItemId == itemId && itm.FAQTagId == tagId);

            if (mappingRecord is null)
                throw new Exception("Mapping record not found");

            await _itemTagMappingRepository.DeleteAsync(mappingRecord);
        }

        #endregion

        #region Methods

        #region Tags

        public async Task InsertFAQTagAsync(FAQTag tag)
        {
            await _tagRepository.InsertAsync(tag);
        }

        public async Task UpdateFAQTagAsync(FAQTag tag)
        {
            await _tagRepository.UpdateAsync(tag);
        }

        public async Task DeleteFAQTagAsync(FAQTag tag)
        {
            await _tagRepository.DeleteAsync(tag);
        }

        public async Task<FAQTag> GetFAQTagByIdAsync(int tagId)
        {
            return await _tagRepository.GetByIdAsync(tagId, cache =>
                _staticCacheManager.PrepareKeyForDefaultCache(NopStationEntityCacheDefaults<FAQTag>.ByIdCacheKey, tagId));
        }

        public async Task<IPagedList<FAQTag>> SearchFAQTagsAsync(string name = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from c in _tagRepository.Table
                        where (string.IsNullOrWhiteSpace(name) || c.Name.Contains(name))
                        select c;

            query = query.OrderByDescending(x => x.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion

        #region Tag mappings

        public virtual async Task InsertFAQItemTagMappingAsync(FAQItemTag tagMapping)
        {
            await _itemTagMappingRepository.InsertAsync(tagMapping);
        }

        public virtual async Task<IList<FAQTag>> GetAllFAQTagsByFAQItemIdAsync(int itemId)
        {
            var query = from itm in _itemTagMappingRepository.Table
                        join t in _tagRepository.Table on itm.FAQTagId equals t.Id
                        where itm.FAQItemId == itemId
                        orderby t.Id
                        select t;

            return await query.ToListAsync();
        }

        public virtual async Task UpdateFAQItemTagsAsync(FAQItem item, string[] tags)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            //product tags
            var existingTags = await GetAllFAQTagsByFAQItemIdAsync(item.Id);
            var tagsToRemove = new List<FAQTag>();
            foreach (var existingTag in existingTags)
            {
                var found = false;
                foreach (var newTag in tags)
                {
                    if (!existingTag.Name.Equals(newTag, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    found = true;
                    break;
                }

                if (!found)
                    tagsToRemove.Add(existingTag);
            }

            foreach (var tag in tagsToRemove)
                await DeleteFAQItemTagMappingAsync(item.Id, tag.Id);

            foreach (var tagName in tags)
            {
                FAQTag tag;
                var tag2 = await GetFAQTagByNameAsync(tagName);
                if (tag2 == null)
                {
                    //add new product tag
                    tag = new FAQTag
                    {
                        Name = tagName
                    };
                    await InsertFAQTagAsync(tag);
                }
                else
                    tag = tag2;

                if (!await FAQItemTagExistsAsync(item, tag.Id))
                    await InsertFAQItemTagMappingAsync(new FAQItemTag { FAQTagId = tag.Id, FAQItemId = item.Id });
            }
        }

        #endregion

        #endregion
    }
}
