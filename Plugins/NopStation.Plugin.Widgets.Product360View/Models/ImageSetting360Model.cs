using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Product360View.Models
{
    public record ImageSetting360Model : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.ProductId")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.IsEnabled")]
        public bool IsEnabled { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.BehaviorTypeId")]
        public int BehaviorTypeId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.IsLoopEnabled")]
        public bool IsLoopEnabled { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.IsZoomEnabled")]
        public bool IsZoomEnabled { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.IsPanoramaEnabled")]
        public bool IsPanoramaEnabled { get; set; }
    }
}
