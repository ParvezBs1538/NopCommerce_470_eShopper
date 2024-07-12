using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.CRM.Zoho.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            AvailableModules = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.Configuration.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.Configuration.Fields.DataCenterId")]
        public int DataCenterId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.Configuration.Fields.ClientId")]
        public string ClientId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.Configuration.Fields.ClientSecret")]
        public string ClientSecret { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.Configuration.Fields.SyncShipment")]
        public bool SyncShipment { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.Configuration.Fields.ShipmentModuleName")]
        public string ShipmentModuleName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.Configuration.Fields.SyncShipmentItem")]
        public bool SyncShipmentItem { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ZohoCRM.Configuration.Fields.ShipmentItemModuleName")]
        public string ShipmentItemModuleName { get; set; }

        public IList<SelectListItem> AvailableModules { get; set; }

        public bool CanSync { get; set; }

        public string OAuthUrl { get; set; }
    }
}
