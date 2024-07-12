using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.CancelOrder.Areas.Admin.Models
{
    public class ConfigurationModel
    {
        public ConfigurationModel()
        {
            CancellableOrderStatuses = new List<int>();
            CancellablePaymentStatuses = new List<int>();
            CancellableShippingStatuses = new List<int>();
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            AvailableShippingStatuses = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.CancelOrder.Configuration.Fields.WidgetZone")]
        public string WidgetZone { get; set; }
        public bool WidgetZone_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CancelOrder.Configuration.Fields.CancellableOrderStatuses")]
        public IList<int> CancellableOrderStatuses { get; set; }
        public bool CancellableOrderStatuses_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CancelOrder.Configuration.Fields.CancellablePaymentStatuses")]
        public IList<int> CancellablePaymentStatuses { get; set; }
        public bool CancellablePaymentStatuses_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CancelOrder.Configuration.Fields.CancellableShippingStatuses")]
        public IList<int> CancellableShippingStatuses { get; set; }
        public bool CancellableShippingStatuses_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }
        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }
        public IList<SelectListItem> AvailableShippingStatuses { get; set; }
    }
}
