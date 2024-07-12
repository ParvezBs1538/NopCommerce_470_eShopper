using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.AllInOneContactUs.Models
{
    public record ARContactUsPublicModel : BaseNopModel
    {
        public string TrackingScript { get; set; }
        public string NameAndEmailScript { get; set; }
    }
}
