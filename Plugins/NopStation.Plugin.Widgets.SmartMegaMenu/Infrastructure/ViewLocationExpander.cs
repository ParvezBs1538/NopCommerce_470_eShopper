using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    private const string MEGAMENU_KEY = "NopStation.Plugin.Widgets.SmartMegaMenu";

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        var megaMenuSettings = NopInstance.Load<SmartMegaMenuSettings>();
        if (megaMenuSettings.EnableMegaMenu && megaMenuSettings.HideDefaultMenu)
            context.Values[MEGAMENU_KEY] = "true";
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocatins)
    {
        if (context.AreaName != "Admin")
        {
            if (context.ViewName == "Components/TopMenu/Default" && context.Values.TryGetValue(MEGAMENU_KEY, out _))
            {
                viewLocatins = new string[] {
                    $"/Plugins/NopStation.Plugin.Widgets.SmartMegaMenu/Views/TopMenuOverride/TopMenu/Default.cshtml"
                };
            }
        }
        return viewLocatins;
    }
}
