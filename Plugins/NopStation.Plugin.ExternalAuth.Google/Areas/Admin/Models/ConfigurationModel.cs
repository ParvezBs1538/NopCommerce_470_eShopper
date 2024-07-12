using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.ExternalAuth.Google.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("NopStation.Plugin.ExternalAuth.Google.ClientKeyIdentifier")]
        public string ClientId { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.ExternalAuth.Google.ClientSecret")]
        public string ClientSecret { get; set; }
    }
}