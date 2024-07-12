using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models
{
    public record SpecialDeliveryOffsetModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.SpecialDeliveryOffsets.Fields.DaysOffset")]
        public int DaysOffset { get; set; }

        public bool Overridden { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.SpecialDeliveryOffsets.Fields.CategoryName")]
        public string CategoryName { get; set; }
    }
}
