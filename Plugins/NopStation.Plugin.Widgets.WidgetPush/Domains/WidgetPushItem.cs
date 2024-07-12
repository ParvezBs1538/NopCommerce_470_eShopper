using System;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Widgets.WidgetPush.Domains
{
    public partial class WidgetPushItem : BaseEntity, ILocalizedEntity, IStoreMappingSupported
    {
        public string Name { get; set; }

        public string WidgetZone { get; set; }

        public int DisplayOrder { get; set; }

        public string Content { get; set; }

        public bool Active { get; set; }

        public DateTime? DisplayStartDateUtc { get; set; }

        public DateTime? DisplayEndDateUtc { get; set; }

        public bool LimitedToStores { get; set; }
    }
}
