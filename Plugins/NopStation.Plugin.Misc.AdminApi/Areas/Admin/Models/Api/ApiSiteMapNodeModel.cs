using System.Collections.Generic;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models.Api;

public class ApiSiteMapNodeModel
{
    public ApiSiteMapNodeModel()
    {
        ChildNodes = [];
    }

    /// <summary>
    /// Gets or sets the system name.
    /// </summary>
    public string SystemName { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the URL.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the child nodes.
    /// </summary>
    public IList<ApiSiteMapNodeModel> ChildNodes { get; set; }

    /// <summary>
    /// Gets or sets the icon class (Font Awesome: http://fontawesome.io/)
    /// </summary>
    public string IconClass { get; set; }

    /// <summary>
    /// Gets or sets the item is visible
    /// </summary>
    public bool Visible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to open url in new tab (window) or not
    /// </summary>
    public bool OpenUrlInBrowser { get; set; }
}
