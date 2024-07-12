using Nop.Core;

namespace NopStation.Plugin.Widgets.CategoryBanners.Domains
{
    public partial class CategoryBanner : BaseEntity
    {
        public int CategoryId { get; set; }

        public int DisplayOrder { get; set; }

        public bool ForMobile { get; set; }

        public int PictureId { get; set; }
    }
}
