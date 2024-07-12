using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.UrlShortener.Areas.Admin.Models
{
    public record ConfigureModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.UrlShortener.Configure.AccessToken")]
        public string AccessToken { get; set; }
        public bool AccessToken_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.UrlShortener.Configure.EnableLog")]
        public bool EnableLog { get; set; }
        public bool EnableLog_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
