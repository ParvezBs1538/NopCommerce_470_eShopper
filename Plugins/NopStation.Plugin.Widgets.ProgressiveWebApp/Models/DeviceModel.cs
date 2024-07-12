using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Models
{
    public record DeviceModel : BaseNopEntityModel
    {
        public string Endpoint { get; set; }

        public string P256dh { get; set; }

        public string Auth { get; set; }
    }
}
