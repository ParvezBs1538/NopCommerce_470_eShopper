using System.Threading.Tasks;
using NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Factories
{
    public interface IFAQItemModelFactory
    {
        Task<FAQItemSearchModel> PrepareFAQItemSearchModelAsync(FAQItemSearchModel searchModel);

        Task<FAQItemListModel> PrepareFAQItemListModelAsync(FAQItemSearchModel searchModel);

        Task<FAQItemModel> PrepareFAQItemModelAsync(FAQItemModel model, FAQItem item,
            bool excludeProperties = false);
    }
}