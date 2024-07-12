using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.WidgetPush.Models;
using NopStation.Plugin.Widgets.WidgetPush.Services;

namespace NopStation.Plugin.Widgets.WidgetPush.Components;

public class WidgetPushViewComponent : NopStationViewComponent
{
    private readonly IWidgetPushItemService _widgetPushItemService;
    private readonly WidgetPushSettings _widgetPushSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IStoreContext _storeContext;

    public WidgetPushViewComponent(IWidgetPushItemService widgetPushItemService,
        WidgetPushSettings widgetPushSettings,
        ILocalizationService localizationService,
        IStoreContext storeContext)
    {
        _widgetPushItemService = widgetPushItemService;
        _widgetPushSettings = widgetPushSettings;
        _localizationService = localizationService;
        _storeContext = storeContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (_widgetPushSettings.HideInPublicStore)
            return Content("");

        var items = await _widgetPushItemService.GetAllWidgetPushItemsAsync(widgetZone, true, DateTime.UtcNow, DateTime.UtcNow, _storeContext.GetCurrentStore().Id);

        var model = new WidgetPushModel()
        {
            WidgetZone = widgetZone
        };
        foreach (var item in items)
        {
            model.WidgetPushItems.Add(new WidgetPushModel.WidgetPushItem()
            {
                Content = await _localizationService.GetLocalizedAsync(item, x => x.Content),
                DisplayOrder = item.DisplayOrder,
                Id = item.Id
            });
        }

        return View(model);
    }
}
