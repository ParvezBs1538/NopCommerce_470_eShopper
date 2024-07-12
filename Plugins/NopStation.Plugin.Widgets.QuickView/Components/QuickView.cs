using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Cms;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Widgets.QuickView.Models;

namespace NopStation.Plugin.Widgets.QuickView.Components;

public class QuickViewViewComponent : NopStationViewComponent
{
    private readonly QuickViewSettings _quickViewSettings;

    public QuickViewViewComponent(QuickViewSettings quickViewSettings)
    {
        _quickViewSettings = quickViewSettings;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var zoomPluginInstalled = await NopPlugin.IsEnabledAsync<IWidgetPlugin>("NopStation.Plugin.Widgets.PictureZoom");

        var model = new PublicModel()
        {
            PictureZoomEnabled = zoomPluginInstalled && _quickViewSettings.EnablePictureZoom
        };

        return View(model);
    }
}
