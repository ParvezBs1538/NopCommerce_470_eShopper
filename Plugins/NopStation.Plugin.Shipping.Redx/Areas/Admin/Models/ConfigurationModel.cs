using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.Redx.Configuration.Fields.FlatShippingCharge")]
        public decimal FlatShippingCharge { get; set; }
        public bool FlatShippingCharge_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.Configuration.Fields.ApiAccessToken")]
        public string ApiAccessToken { get; set; }
        public bool ApiAccessToken_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.Configuration.Fields.ParcelTrackUrl")]
        public string ParcelTrackUrl { get; set; }
        public bool ParcelTrackUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.Configuration.Fields.ShipmentEventsUrl")]
        public string ShipmentEventsUrl { get; set; }
        public bool ShipmentEventsUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.Configuration.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.Configuration.Fields.SandboxUrl")]
        public string SandboxUrl { get; set; }
        public bool SandboxUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.Configuration.Fields.BaseUrl")]
        public string BaseUrl { get; set; }
        public bool BaseUrl_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}