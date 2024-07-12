using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Services
{
    public interface IFAQTagService
    {
        Task InsertFAQTagAsync(FAQTag tag);

        Task UpdateFAQTagAsync(FAQTag tag);

        Task DeleteFAQTagAsync(FAQTag tag);

        Task<FAQTag> GetFAQTagByIdAsync(int tagId);

        Task<IPagedList<FAQTag>> SearchFAQTagsAsync(string name = "", int pageIndex = 0, int pageSize = int.MaxValue);

        Task InsertFAQItemTagMappingAsync(FAQItemTag tagMapping);

        Task<IList<FAQTag>> GetAllFAQTagsByFAQItemIdAsync(int itemId);

        Task UpdateFAQItemTagsAsync(FAQItem item, string[] tags);
    }
}