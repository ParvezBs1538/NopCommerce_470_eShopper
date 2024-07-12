using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Models
{
    public record OCarouselItemSearchModel : BaseSearchModel
    {
        public int OCarouselId { get; set; }
    }
}
