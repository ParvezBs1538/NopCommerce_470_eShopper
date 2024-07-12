using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Models
{
    public record AbandonmentDetailsViewModel : BaseNopEntityModel
    {
        public AbandonmentListViewModel AbandonmentInfo { get; set; }
        public CustomerAbandonmentInfoModel CustomerAbandonmentInfo { get; set; }
    }
}
