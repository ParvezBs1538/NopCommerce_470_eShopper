using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models
{
    public record DeliveryCapacityConfigurationModel : BaseNopEntityModel
    {
        public DeliveryCapacityConfigurationModel()
        {
            DeliveryCapacities = new Dictionary<int, DeliveryCapacityModel>();
            ShippingMethods = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Fields.ShippingMethod")]
        public int ShippingMethodId { get; set; }

        public IList<SelectListItem> ShippingMethods { get; set; }

        public IDictionary<int, DeliveryCapacityModel> DeliveryCapacities { get; set; }
    }
}
