using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.Widgets.OCarousels.Models
{
    public record OCarouselModel : BaseNopEntityModel
    {
        public OCarouselModel()
        {
            Products = new List<ProductOverviewModel>();
            Manufacturers = new List<OCarouselManufacturerModel>();
            Categories = new List<OCarouselCategoryModel>();
        }

        public string Title { get; set; }

        public bool DisplayTitle { get; set; }

        public bool ShowBackgroundPicture { get; set; }

        public string BackgroundPictureUrl { get; set; }

        public bool AutoPlay { get; set; }

        public bool RTL { get; set; }

        public string CustomCssClass { get; set; }

        public bool Loop { get; set; }

        public int StartPosition { get; set; }

        public bool Center { get; set; }

        public bool Nav { get; set; }

        public bool LazyLoad { get; set; }

        public int LazyLoadEager { get; set; }

        public int AutoPlayTimeout { get; set; }

        public bool AutoPlayHoverPause { get; set; }

        public CarouselType CarouselType { get; set; }

        public IList<ProductOverviewModel> Products { get; set; }

        public IList<OCarouselManufacturerModel> Manufacturers { get; set; }

        public IList<OCarouselCategoryModel> Categories { get; set; }

        public record OCarouselManufacturerModel : BaseNopModel
        {
            public OCarouselManufacturerModel()
            {
                PictureModel = new PictureModel();
            }

            public string Name { get; set; }

            public string SeName { get; set; }

            public PictureModel PictureModel { get; set; }
        }

        public record OCarouselCategoryModel : BaseNopModel
        {
            public OCarouselCategoryModel()
            {
                PictureModel = new PictureModel();
            }

            public string Name { get; set; }

            public string SeName { get; set; }

            public PictureModel PictureModel { get; set; }
        }
    }
}
