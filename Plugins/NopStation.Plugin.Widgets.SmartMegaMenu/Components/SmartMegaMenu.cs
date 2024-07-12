using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.SmartMegaMenu.Factories;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Components;

public class SmartMegaMenuViewComponent : NopStationViewComponent
{
    private readonly IMegaMenuModelFactory _megaMenuModelFactory;
    private readonly SmartMegaMenuSettings _smartMegaMenuSettings;

    public SmartMegaMenuViewComponent(IMegaMenuModelFactory megaMenuModelFactory,
        SmartMegaMenuSettings smartMegaMenuSettings)
    {
        _megaMenuModelFactory = megaMenuModelFactory;
        _smartMegaMenuSettings = smartMegaMenuSettings;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!_smartMegaMenuSettings.EnableMegaMenu)
            return Content("");

        var model = await _megaMenuModelFactory.PrepareMegaMenuModelsAsync(widgetZone);

        return View(model);
    }
}
