using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using System;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Domains
{
    public class DeliverySlot : BaseEntity, ILocalizedEntity, ISoftDeletedEntity, IStoreMappingSupported
    {
        public string TimeSlot { get; set; }

        public int DisplayOrder { get; set; }

        public bool Deleted { get; set; }

        public bool Active { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public bool LimitedToShippingMethod { get; set; }

        public bool LimitedToStores { get; set; }
    }
}
