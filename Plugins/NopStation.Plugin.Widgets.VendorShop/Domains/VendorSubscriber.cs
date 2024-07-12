using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.VendorShop.Domains
{
    public class VendorSubscriber : BaseEntity
    {
        public int VendorId { get; set; }
        public int CustomerId { get; set; }
        public int StoreId { get; set; }
        public DateTime SubscribedOn { get; set; }
    }
}
