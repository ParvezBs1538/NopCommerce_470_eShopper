using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models
{
    public record SpecialDeliveryCapacityModel : BaseNopEntityModel, IStoreMappingSupportedModel
    {
        public SpecialDeliveryCapacityModel()
        {
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
            AvailableDeliverySlots = new List<SelectListItem>();
            AvailableShippingMethods = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.SpecialDate")]
        [UIHint("DateNullable")]
        public DateTime SpecialDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.SpecialDate")]
        public string SpecialDateStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.DeliverySlot")]
        public int DeliverySlotId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.ShippingMethod")]
        public int ShippingMethodId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.DeliverySlot")]
        public string DeliverySlot { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.Capacity")]
        public int Capacity { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.Note")]
        public string Note { get; set; }

        public string MappedStoreNames { get; set; }

        //store mapping
        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableDeliverySlots { get; set; }

        public IList<SelectListItem> AvailableShippingMethods { get; set; }
    }
}
