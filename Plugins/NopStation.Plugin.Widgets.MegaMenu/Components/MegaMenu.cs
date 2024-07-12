using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.MegaMenu.Factories;

namespace NopStation.Plugin.Widgets.MegaMenu.Components;

public class MegaMenuViewComponent : NopStationViewComponent
{
    private readonly MegaMenuSettings _megaMenuSettings;
    private readonly IMegaMenuModelFactory _megaMenuModelFactory;

    public MegaMenuViewComponent(MegaMenuSettings megaMenuSettings,
        IMegaMenuModelFactory megaMenuModelFactory)
    {
        _megaMenuSettings = megaMenuSettings;
        _megaMenuModelFactory = megaMenuModelFactory;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = await _megaMenuModelFactory.PrepareMegaMenuModelAsync();
        return View(model);
    }
}