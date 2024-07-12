using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.CRM.Zoho.Areas.Admin.Models
{
    public record MapShipmentFieldsModel : BaseNopModel
    {
        public MapShipmentFieldsModel()
        {
            AvailableFields = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.OrderField")]
        public string OrderField { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.TrackingNumberField")]
        public string TrackingNumberField { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.WeightField")]
        public string WeightField { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.ShippedDateUtcField")]
        public string ShippedDateUtcField { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.DeliveryDateUtcField")]
        public string DeliveryDateUtcField { get; set; }

        public bool CloseWindow { get; set; }

        public IList<SelectListItem> AvailableFields { get; set; }
    }
}
