using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models
{
    public record OrderDeliverySlotSearchModel : BaseSearchModel
    {
        public OrderDeliverySlotSearchModel()
        {
            AvailableShippingMethods = new List<SelectListItem>();
            AvailableDeliverySlots = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.SearchStartDate")]
        [UIHint("DateNullable")]
        public DateTime? SearchStartDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.SearchEndTime")]
        [UIHint("DateNullable")]
        public DateTime? SearchEndTime { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.SearchShippingMethod")]
        public int SearchShippingMethodId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.OrderDeliverySlots.List.SearchDeliverySlot")]
        public int SearchDeliverySlotId { get; set; }

        public List<SelectListItem> AvailableShippingMethods { get; set; }

        public IList<SelectListItem> AvailableDeliverySlots { get; set; }
    }
}
