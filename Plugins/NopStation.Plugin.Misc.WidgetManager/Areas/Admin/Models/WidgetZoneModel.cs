using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

public partial record WidgetZoneModel : BaseNopEntityModel
{
    public WidgetZoneModel()
    {
        AvaliableWidgetZones = new List<SelectListItem>();
    }

    public string EntityName { get; set; }
    public int EntityId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.WidgetZones.Fields.WidgetZone")]
    public string WidgetZone { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.WidgetZones.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    public IList<SelectListItem> AvaliableWidgetZones { get; set; }
}
