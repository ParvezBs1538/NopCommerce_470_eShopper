using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models
{
    public record DeliverySlotModel : BaseNopEntityModel, ILocalizedModel<DeliverySlotLocalizedModel>, IStoreMappingSupportedModel
    {
        public DeliverySlotModel()
        {
            Locales = new List<DeliverySlotLocalizedModel>();
            SelectedShippingMethodIds = new List<int>();
            AvailableShippingMethods = new List<SelectListItem>();
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.TimeSlot")]
        public string TimeSlot { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.LimitedToShippingMethods")]
        public IList<int> SelectedShippingMethodIds { get; set; }

        public IList<SelectListItem> AvailableShippingMethods { get; set; }

        //store mapping
        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<DeliverySlotLocalizedModel> Locales { get; set; }
    }

    public partial class DeliverySlotLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.TimeSlot")]
        public string TimeSlot { get; set; }
    }
}
