using Nop.Core;

namespace NopStation.Plugin.Widgets.VendorShop.Domains
{
    public class VendorFeatureMapping : BaseEntity
    {
        public int VendorId { get; set; }
        public bool Enable { get; set; }
    }
}
