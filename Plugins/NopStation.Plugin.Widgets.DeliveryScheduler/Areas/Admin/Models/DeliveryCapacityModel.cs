using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models
{
    public record DeliveryCapacityModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.DeliverySlot")]
        public int DeliverySlotId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.DeliverySlot")]
        public string DeliverySlot { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day1Capacity")]
        public int Day1Capacity { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day2Capacity")]
        public int Day2Capacity { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day3Capacity")]
        public int Day3Capacity { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day4Capacity")]
        public int Day4Capacity { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day5Capacity")]
        public int Day5Capacity { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day6Capacity")]
        public int Day6Capacity { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.Day7Capacity")]
        public int Day7Capacity { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.ShippingMethod")]
        public int ShippingMethodId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.ShippingMethod")]
        public string ShippingMethod { get; set; }
    }
}
