using Nop.Core;

namespace NopStation.Plugin.Shipping.DHL.Domain
{
    public class DHLService : BaseEntity
    {
        public string Name { get; set; }

        public string GlobalProductCode { get; set; }

        public bool IsActive { get; set; }
    }
}
