using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Services.Cms;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Widgets.QuickView.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    private const string THEME_KEY = "nop.themename";
    private const string PICTUREZOOM_KEY = "nopstation.quickview.picturezoom";

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        if (context.ControllerName == "QuickView" && context.ViewName == "_ProductDetailsPictures" &&
            NopPlugin.IsEnabledAsync<IWidgetPlugin>("NopStation.Plugin.Widgets.PictureZoom").Result &&
            NopInstance.Load<QuickViewSettings>().EnablePictureZoom)
        {
            context.Values[PICTUREZOOM_KEY] = "true";
        }
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (context.AreaName != "Admin")
        {
            if (DisplayZoomPictureView(context))
            {
                viewLocations = new string[] {
                    $"/Plugins/NopStation.Plugin.Widgets.PictureZoom/Views/Shared/PictureZoom.cshtml"
                }.Concat(viewLocations);

                if (context.Values.TryGetValue(THEME_KEY, out var theme))
                {
                    viewLocations = new string[] {
                        $"/Plugins/NopStation.Plugin.Widgets.PictureZoom/Themes/{theme}/Views/Shared/PictureZoom.cshtml"
                    }.Concat(viewLocations);
                }
            }
        }

        return viewLocations;
    }

    private static bool DisplayZoomPictureView(ViewLocationExpanderContext context)
    {
        if (context.Values.TryGetValue(PICTUREZOOM_KEY, out var val))
            return val == "true";

        return false;
    }
}
