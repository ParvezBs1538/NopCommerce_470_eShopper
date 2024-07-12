using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Http.Extensions;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Announcement.Domains;
using NopStation.Plugin.Widgets.Announcement.Models;
using NopStation.Plugin.Widgets.Announcement.Services;

namespace NopStation.Plugin.Widgets.Announcement.Components;

public class AnnouncementViewComponent : NopStationViewComponent
{
    private readonly IAnnouncementItemService _announcementItemService;
    private readonly AnnouncementSettings _announcementSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IStoreContext _storeContext;

    public AnnouncementViewComponent(IAnnouncementItemService announcementItemService,
        AnnouncementSettings announcementSettings,
        ILocalizationService localizationService,
        IStoreContext storeContext)
    {
        _announcementItemService = announcementItemService;
        _announcementSettings = announcementSettings;
        _localizationService = localizationService;
        _storeContext = storeContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!_announcementSettings.EnablePlugin)
            return Content("");

        if (_announcementSettings.AllowCustomersToClose && await HttpContext.Session.GetAsync<bool>(AnnouncementDefaults.Closed))
            return Content("");

        var items = await _announcementItemService.GetAllAnnouncementItemsAsync(
            storeId: _storeContext.GetCurrentStore().Id,
            validScheduleOnly: true);

        var model = new AnnouncementModel()
        {
            WidgetZone = widgetZone,
            AllowCustomersToClose = _announcementSettings.AllowCustomersToClose,
            AllowCustomersToMinimize = _announcementSettings.AllowCustomersToMinimize,
            DisplayType = (DisplayType)_announcementSettings.DisplayTypeId,
            ItemSeparator = _announcementSettings.ItemSeparator,
            AnnouncementBarMinimized = await HttpContext.Session.GetAsync<bool>(AnnouncementDefaults.Minimized)
        };

        foreach (var item in items)
        {
            model.AnnouncementItems.Add(new AnnouncementModel.AnnouncementItem()
            {
                Title = await _localizationService.GetLocalizedAsync(item, x => x.Title),
                Description = await _localizationService.GetLocalizedAsync(item, x => x.Description),
                DisplayOrder = item.DisplayOrder,
                Id = item.Id,
                Color = item.Color
            });
        }

        return View(model);
    }
}
