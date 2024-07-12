using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AutoCancelOrder.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            ApplyOnOrderStatuses = new List<int>();
            ApplyOnShippingStatuses = new List<int>();
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailableShippingStatuses = new List<SelectListItem>();
            AvailableUnits = new List<SelectListItem>();
            AvailablePaymentMethods = new List<SelectListItem>();
            ApplyOnPaymentMethods = new List<PaymentMethodOffset>();
        }

        [NopResourceDisplayName("Admin.NopStation.AutoCancelOrder.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AutoCancelOrder.Configuration.Fields.SendNotificationToCustomer")]
        public bool SendNotificationToCustomer { get; set; }
        public bool SendNotificationToCustomer_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AutoCancelOrder.Configuration.Fields.ApplyOnOrderStatuses")]
        public IList<int> ApplyOnOrderStatuses { get; set; }
        public bool ApplyOnOrderStatuses_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AutoCancelOrder.Configuration.Fields.ApplyOnShippingStatuses")]
        public IList<int> ApplyOnShippingStatuses { get; set; }
        public bool ApplyOnShippingStatuses_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AutoCancelOrder.Configuration.Fields.ApplyOnPaymentMethods")]
        public IList<PaymentMethodOffset> ApplyOnPaymentMethods { get; set; }
        public bool ApplyOnPaymentMethods_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }
        public IList<SelectListItem> AvailableShippingStatuses { get; set; }
        public IList<SelectListItem> AvailablePaymentMethods { get; set; }
        public IList<SelectListItem> AvailableUnits { get; set; }

        public string[] SystemName { get; set; }
        public int[] Offset { get; set; }
        public int[] UnitTypeId { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
