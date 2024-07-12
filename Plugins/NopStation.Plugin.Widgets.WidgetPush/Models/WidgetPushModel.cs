using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.WidgetPush.Models
{
    public class WidgetPushModel
    {
        public WidgetPushModel()
        {
            WidgetPushItems = new List<WidgetPushItem>();
        }

        public string WidgetZone { get; set; }

        public IList<WidgetPushItem> WidgetPushItems { get; set; }

        public record WidgetPushItem : BaseNopEntityModel
        {
            public int DisplayOrder { get; set; }

            public string Content { get; set; }
        }
    }
}
