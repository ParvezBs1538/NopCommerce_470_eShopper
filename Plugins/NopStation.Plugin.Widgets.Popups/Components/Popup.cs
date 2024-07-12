using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Popups.Factories;

namespace NopStation.Plugin.Widgets.Popups.Components;

public class PopupViewComponent : NopStationViewComponent
{
    private readonly IPopupModelFactory _popupModelFactory;

    public PopupViewComponent(IPopupModelFactory popupModelFactory)
    {
        _popupModelFactory = popupModelFactory;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var model = await _popupModelFactory.PreparePopupPublicModelAsync();
        return View(model);
    }
}
