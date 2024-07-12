using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models
{
    public record OrderDeliverySlotModel : BaseNopEntityModel
    {
        public OrderDeliverySlotModel()
        {
            AvailableDeliverySlots = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.Fields.DeliveryDate")]
        [UIHint("Date")]
        public DateTime DeliveryDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.Fields.DeliverySlot")]
        public int DeliverySlotId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.Fields.DeliverySlot")]
        public string DeliverySlot { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.Fields.ShippingMethod")]
        public string ShippingMethod { get; set; }

        public IList<SelectListItem> AvailableDeliverySlots { get; set; }
    }
}
