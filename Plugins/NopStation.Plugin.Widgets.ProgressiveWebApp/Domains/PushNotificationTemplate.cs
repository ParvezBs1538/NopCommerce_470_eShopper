using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Domains
{
    public class PushNotificationTemplate : BaseEntity, ILocalizedEntity, IStoreMappingSupported
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public bool UseDefaultIcon { get; set; }

        public int IconId { get; set; }

        public int ImageId { get; set; }

        public string Url { get; set; }

        public bool Active { get; set; }

        public bool LimitedToStores { get; set; }
    }
}
