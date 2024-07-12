using System.Threading.Tasks;
using NopStation.Plugin.Widgets.WidgetPush.Areas.Admin.Models;
using NopStation.Plugin.Widgets.WidgetPush.Domains;

namespace NopStation.Plugin.Widgets.WidgetPush.Areas.Admin.Factories
{
    public interface IWidgetPushItemModelFactory
    {
        WidgetPushItemSearchModel PrepareWidgetPushItemSearchModel(WidgetPushItemSearchModel searchModel);

        Task<WidgetPushItemListModel> PrepareWidgetPushItemListModelAsync(WidgetPushItemSearchModel searchModel);

        Task<WidgetPushItemModel> PrepareWidgetPushItemModelAsync(WidgetPushItemModel model, WidgetPushItem widgetPushItem, bool excludeProperties = false);
    }
}