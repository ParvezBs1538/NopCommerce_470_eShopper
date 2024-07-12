using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Core.Infrastructure;

namespace NopStation.Plugin.Widgets.MegaMenu.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    private const string THEME_KEY = "nop.themename";
    private const string MEGAMENU_KEY = "nopstation.megamenu";

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        if (context.ViewName == "Components/TopMenu/Default")
        {
            var megaMenuSettings = EngineContext.Current.Resolve<MegaMenuSettings>();
            if (megaMenuSettings.EnableMegaMenu)
                context.Values[MEGAMENU_KEY] = "true";
        }
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (context.AreaName == "Admin")
        {
            viewLocations = new string[] {
                $"/Plugins/NopStation.Plugin.Widgets.MegaMenu/Areas/Admin/Views/{{1}}/{{0}}.cshtml",
                $"/Plugins/NopStation.Plugin.Widgets.MegaMenu/Areas/Admin/Views/Shared/{{0}}.cshtml"
            }.Concat(viewLocations);
        }
        else
        {
            if (context.Values.TryGetValue(MEGAMENU_KEY, out _))
                viewLocations = new string[] { "~/Plugins/NopStation.Plugin.Widgets.MegaMenu/Views/Shared/TopMenu.cshtml" };
            else if (context.Values.TryGetValue(THEME_KEY, out string theme))
            {
                viewLocations = new string[] {
                    $"/Plugins/NopStation.Plugin.Widgets.MegaMenu/Views/{{1}}/{{0}}.cshtml",
                    $"/Plugins/NopStation.Plugin.Widgets.MegaMenu/Views/Shared/{{0}}.cshtml"
                }.Concat(viewLocations);

                viewLocations = new string[] {
                    $"/Plugins/NopStation.Plugin.Widgets.MegaMenu/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                    $"/Plugins/NopStation.Plugin.Widgets.MegaMenu/Themes/{theme}/Views/Shared/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
        }
        return viewLocations;
    }
}
