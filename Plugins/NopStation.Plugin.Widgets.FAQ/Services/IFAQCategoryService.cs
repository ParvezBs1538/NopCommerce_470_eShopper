using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Services
{
    public interface IFAQCategoryService
    {
        Task InsertFAQCategoryAsync(FAQCategory category);

        Task UpdateFAQCategoryAsync(FAQCategory category);

        Task DeleteFAQCategoryAsync(FAQCategory category);

        Task<FAQCategory> GetFAQCategoryByIdAsync(int categoryId);

        Task<IPagedList<FAQCategory>> SearchFAQCategoriesAsync(string name = "", int storeId = 0,
            bool? published = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task DeleteFAQItemCategoryAsync(FAQItemCategory categoryMapping);

        Task<IList<FAQItemCategory>> GetFAQItemCategoriesByItemIdAsync(int itemId, bool showHidden = false);

        IList<int> GetFAQItemCategoryIds(FAQItem fAQItem);

        FAQItemCategory FindFAQItemCategory(IList<FAQItemCategory> source, int itemId, int categoryId);

        Task<IPagedList<FAQItemCategory>> GetFAQItemCategoriesByCategoryIdAsync(int categoryId,
               int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        Task InsertFAQItemCategoryAsync(FAQItemCategory itemCategory);
    }
}