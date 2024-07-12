using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Services
{
    public interface IFAQItemService
    {
        Task InsertFAQItemAsync(FAQItem item);

        Task UpdateFAQItemAsync(FAQItem item);

        Task DeleteFAQItemAsync(FAQItem item);

        Task<FAQItem> GetFAQItemByIdAsync(int itemId);

        Task<IPagedList<FAQItem>> SearchFAQItemsAsync(string keyword = "", int categoryId = 0, int tagId = 0,
            bool? published = null, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}