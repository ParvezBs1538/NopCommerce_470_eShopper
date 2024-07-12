using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Widgets.PictureZoom.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    private const string THEME_KEY = "nop.themename";
    private const string PICTUREZOOM_KEY = "nopstation.quickview.picturezoom";

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        if (context.ControllerName == "Product" && context.ViewName == "_ProductDetailsPictures")
        {
            var pluginService = NopInstance.Load<IPluginService>();
            var storeContext = NopInstance.Load<IStoreContext>();
            var workContext = NopInstance.Load<IWorkContext>();
            var widgetPluginManager = NopInstance.Load<IWidgetPluginManager>();
            var pictureZoomSettings = EngineContext.Current.Resolve<PictureZoomSettings>();
            var pluginDescriptor = pluginService.GetPluginDescriptorBySystemNameAsync<IWidgetPlugin>("NopStation.Plugin.Widgets.PictureZoom",
                LoadPluginsMode.InstalledOnly, workContext.GetCurrentCustomerAsync().Result, storeContext.GetCurrentStoreAsync().Result.Id).Result;

            if (pluginDescriptor != null && widgetPluginManager.IsPluginActive(pluginDescriptor.Instance<IWidgetPlugin>()) && pictureZoomSettings.EnablePictureZoom == true)
                context.Values[PICTUREZOOM_KEY] = "true";
        }
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (context.AreaName != "Admin")
        {
            if (DisplayZoomPictureView(context))
            {
                viewLocations = new string[] { $"/Plugins/NopStation.Plugin.Widgets.PictureZoom/Views/Shared/PictureZoom.cshtml" };

                if (context.Values.TryGetValue(THEME_KEY, out string theme))
                {
                    viewLocations = new string[] {
                        $"/Plugins/NopStation.Plugin.Widgets.PictureZoom/Themes/{theme}/Views/Shared/PictureZoom.cshtml"
                    }.Concat(viewLocations);
                }
            }
        }
        return viewLocations;
    }

    private bool DisplayZoomPictureView(ViewLocationExpanderContext context)
    {
        if (context.Values.TryGetValue(PICTUREZOOM_KEY, out var val))
            return val == "true";

        return false;
    }
}
