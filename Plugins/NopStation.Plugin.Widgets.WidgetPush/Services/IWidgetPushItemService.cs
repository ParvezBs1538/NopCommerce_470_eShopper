using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.WidgetPush.Domains;

namespace NopStation.Plugin.Widgets.WidgetPush.Services
{
    public interface IWidgetPushItemService
    {
        Task DeleteWidgetPushItemAsync(WidgetPushItem widgetPushItem);

        Task InsertWidgetPushItemAsync(WidgetPushItem widgetPushItem);

        Task UpdateWidgetPushItemAsync(WidgetPushItem widgetPushItem);

        Task<WidgetPushItem> GetWidgetPushItemByIdAsync(int widgetPushItemId);

        Task<IPagedList<WidgetPushItem>> GetAllWidgetPushItemsAsync(string widgetZone = "", bool? active = null,
            DateTime? startDate = null, DateTime? endDate = null, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<IList<string>> GetAllWidgetZonesAsync();
    }
}