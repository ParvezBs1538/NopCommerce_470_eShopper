using Nop.Core;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Domains
{
    public class DeliveryCapacity : BaseEntity
    {
        public int DeliverySlotId { get; set; }

        public int Day1Capacity { get; set; }

        public int Day2Capacity { get; set; }

        public int Day3Capacity { get; set; }

        public int Day4Capacity { get; set; }

        public int Day5Capacity { get; set; }

        public int Day6Capacity { get; set; }

        public int Day7Capacity { get; set; }

        public int ShippingMethodId { get; set; }
    }
}
