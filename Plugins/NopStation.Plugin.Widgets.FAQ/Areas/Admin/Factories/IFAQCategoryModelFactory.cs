using System.Threading.Tasks;
using NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Factories
{
    public interface IFAQCategoryModelFactory
    {
        Task<FAQCategorySearchModel> PrepareFAQCategorySearchModelAsync(FAQCategorySearchModel searchModel);

        Task<FAQCategoryListModel> PrepareFAQCategoryListModelAsync(FAQCategorySearchModel searchModel);

        Task<FAQCategoryModel> PrepareFAQCategoryModelAsync(FAQCategoryModel model, FAQCategory category,
            bool excludeProperties = false);
    }
}