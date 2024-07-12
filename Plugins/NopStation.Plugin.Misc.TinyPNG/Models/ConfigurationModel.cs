using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.TinyPNG.Models
{
   public record ConfigurationModel : BaseNopEntityModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TinyPNG.Configuration.Fields.Enable")]
      
        public bool TinyPNGEnable { get; set; }
        public bool TinyPNGEnable_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TinyPNG.Configuration.Fields.Keys")]
        public string Keys { get; set; }
        public bool Keys_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TinyPNG.Configuration.Fields.ApiUrl")]
        public string ApiUrl { get; set; }
        public bool ApiUrl_OverrideForStore { get; set; }

    }
}
