using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    private const string THEME_KEY = "nop.themename";
    private readonly string _folderName;
    private readonly bool _rootAdmin;
    private readonly bool _excludepublicView;

    public ViewLocationExpander(string folderName, bool rootAdmin, bool excludepublicView)
    {
        _folderName = folderName;
        _rootAdmin = rootAdmin;
        _excludepublicView = excludepublicView;
    }

    public void PopulateValues(ViewLocationExpanderContext context)
    {
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (context.AreaName == "Admin")
        {
            if (!_rootAdmin)
            {
                viewLocations = new[] {
                    $"/Plugins/{_folderName}/Areas/Admin/Views/{{1}}/{{0}}.cshtml",
                    $"/Plugins/{_folderName}/Areas/Admin/Views/Shared/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
            else
            {
                viewLocations = new[] {
                    $"/Plugins/{_folderName}/Views/{{1}}/{{0}}.cshtml",
                    $"/Plugins/{_folderName}/Views/Shared/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
        }
        else if (!_excludepublicView)
        {
            viewLocations = new[] {
                $"/Plugins/{_folderName}/Views/{{1}}/{{0}}.cshtml",
                $"/Plugins/{_folderName}/Views/Shared/{{0}}.cshtml"
            }.Concat(viewLocations);

            if (context.Values.TryGetValue(THEME_KEY, out string theme))
            {
                viewLocations = new[] {
                    $"/Plugins/{_folderName}/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                    $"/Plugins/{_folderName}/Themes/{theme}/Views/Shared/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
        }

        return viewLocations;
    }
}
