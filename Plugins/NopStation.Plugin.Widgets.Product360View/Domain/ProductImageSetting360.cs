using Nop.Core;

namespace NopStation.Plugin.Widgets.Product360View.Domain
{
    public class ProductImageSetting360 : BaseEntity
    {
        public int ProductId { get; set; }
        public bool IsEnabled { get; set; }
        public int BehaviorTypeId { get; set; }
        public bool IsLoopEnabled { get; set; }
        public bool IsZoomEnabled { get; set; }
        public bool IsPanoramaEnabled { get; set; }
    }
}
