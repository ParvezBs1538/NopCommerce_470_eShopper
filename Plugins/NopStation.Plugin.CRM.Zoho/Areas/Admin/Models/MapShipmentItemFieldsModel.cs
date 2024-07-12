using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.CRM.Zoho.Areas.Admin.Models
{
    public record MapShipmentItemFieldsModel : BaseNopModel
    {
        public MapShipmentItemFieldsModel()
        {
            AvailableFields = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.MapShipmentItemFields.Fields.ShipmentField")]
        public string ShipmentField { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.MapShipmentItemFields.Fields.ProductField")]
        public string ProductField { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.MapShipmentItemFields.Fields.QuantityField")]
        public string QuantityField { get; set; }

        public bool CloseWindow { get; set; }

        public IList<SelectListItem> AvailableFields { get; set; }
    }
}
