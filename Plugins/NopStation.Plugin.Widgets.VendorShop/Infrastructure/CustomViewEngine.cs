using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Web.Framework;

namespace NopStation.Plugin.Widgets.VendorShop.Infrastructure
{
    public class CustomViewEngine : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName == AreaNames.ADMIN)
            {
                viewLocations = new string[] {
                    $"~/Plugins/NopStation.Plugin.Widgets.VendorShop/Areas/Admin/Views/Shared/{{0}}.cshtml",
                    $"~/Plugins/NopStation.Plugin.Widgets.VendorShop/Areas/Admin/Views/{{1}}/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
            else
            {
                viewLocations = new string[] {
                    $"~/Plugins/NopStation.Plugin.Widgets.VendorShop/Views/Shared/{{0}}.cshtml",
                    $"~/Plugins/NopStation.Plugin.Widgets.VendorShop/Views/{{1}}/{{0}}.cshtml",
                }.Concat(viewLocations);

                if (context.Values.TryGetValue(THEME_KEY, out string theme))
                {
                    viewLocations = new string[] {
                        $"~/Plugins/NopStation.Plugin.Widgets.VendorShop/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                        $"~/Plugins/NopStation.Plugin.Widgets.VendorShop/Themes/{theme}/Views/{{1}}/{{0}}.cshtml"
                    }.Concat(viewLocations);
                }
            }

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }
    }
}
