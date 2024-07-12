using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.PinterestAnalytics.Models
{
    public partial record CustomEventModel : BaseNopModel
    {
        #region Ctor

        public CustomEventModel()
        {
            WidgetZonesIds = new List<int>();
            WidgetZones = new List<string>();
            AvailableWidgetZones = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Configuration.CustomEvents.Fields.EventName")]
        public string EventName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Configuration.CustomEvents.Fields.WidgetZones")]
        public IList<int> WidgetZonesIds { get; set; }
        public IList<string> WidgetZones { get; set; }
        public IList<SelectListItem> AvailableWidgetZones { get; set; }
        public string WidgetZonesName { get; set; }

        #endregion
    }
}