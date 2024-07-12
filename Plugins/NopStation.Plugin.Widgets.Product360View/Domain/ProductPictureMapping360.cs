using Nop.Core;

namespace NopStation.Plugin.Widgets.Product360View.Domain
{
    public class ProductPictureMapping360 : BaseEntity
    {
        public int ProductId { get; set; }
        public int PictureId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPanorama { get; set; }
    }
}
