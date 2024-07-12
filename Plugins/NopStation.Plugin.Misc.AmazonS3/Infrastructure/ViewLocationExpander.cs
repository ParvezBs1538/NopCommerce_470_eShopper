using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;

namespace NopStation.Plugin.Misc.AmazonS3.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName == "Admin" && context.ViewName == "EditorTemplates/RichEditor")
                viewLocations = new string[] { "~/Plugins/NopStation.Plugin.Misc.AmazonS3/Views/{0}.cshtml" };

            return viewLocations;
        }
    }
}
