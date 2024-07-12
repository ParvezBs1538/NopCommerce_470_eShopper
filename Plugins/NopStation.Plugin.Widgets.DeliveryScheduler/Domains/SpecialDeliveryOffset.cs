using Nop.Core;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Domains
{
    public class SpecialDeliveryOffset : BaseEntity
    {
        public int CategoryId { get; set; }

        public int DaysOffset { get; set; }
    }
}
