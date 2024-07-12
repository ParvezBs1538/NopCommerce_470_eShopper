using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Models;

public record CarouselModel : BaseNopEntityModel
{
    public CarouselModel()
    {
        Products = new List<ProductOverviewModel>();
        Manufacturers = new List<ManufacturerModel>();
        Categories = new List<CategoryModel>();
        Vendors = new List<VendorModel>();
        Pictures = new List<CarouselPictureModel>();
    }

    public string Title { get; set; }

    public bool DisplayTitle { get; set; }

    public string CustomUrl { get; set; }

    public bool ShowBackground { get; set; }

    public string BackgroundPictureUrl { get; set; }

    public string BackgroundColor { get; set; }

    public bool AutoPlay { get; set; }

    public string CustomCssClass { get; set; }

    public bool Loop { get; set; }

    public int StartPosition { get; set; }

    public bool Center { get; set; }

    public bool Navigation { get; set; }

    public bool LazyLoad { get; set; }

    public int AutoPlayTimeout { get; set; }

    public bool AutoPlayHoverPause { get; set; }

    public bool KeyboardControl { get; set; }

    public bool KeyboardControlOnlyInViewport { get; set; }

    public bool Pagination { get; set; }

    public bool PaginationClickable { get; set; }

    public bool PaginationDynamicBullets { get; set; }

    public int PaginationDynamicMainBullets { get; set; }

    public PaginationType PaginationType { get; set; }

    public CarouselType CarouselType { get; set; }

    public IList<ProductOverviewModel> Products { get; set; }

    public IList<ManufacturerModel> Manufacturers { get; set; }

    public IList<CategoryModel> Categories { get; set; }

    public IList<VendorModel> Vendors { get; set; }

    public IList<CarouselPictureModel> Pictures { get; set; }

    public BackgroundType BackgroundType { get; set; }

    public record ManufacturerModel : BaseNopModel
    {
        public ManufacturerModel()
        {
            PictureModel = new PictureModel();
        }

        public string Name { get; set; }

        public string SeName { get; set; }

        public PictureModel PictureModel { get; set; }
    }

    public record CategoryModel : BaseNopModel
    {
        public CategoryModel()
        {
            PictureModel = new PictureModel();
        }

        public string Name { get; set; }

        public string SeName { get; set; }

        public PictureModel PictureModel { get; set; }
    }

    public record VendorModel : BaseNopModel
    {
        public VendorModel()
        {
            PictureModel = new PictureModel();
        }

        public string Name { get; set; }

        public string SeName { get; set; }

        public PictureModel PictureModel { get; set; }
    }

    public record CarouselPictureModel : BaseNopModel
    {
        public CarouselPictureModel()
        {
            PictureModel = new PictureModel();
        }

        public string Label { get; set; }

        public string RedirectUrl { get; set; }

        public PictureModel PictureModel { get; set; }
    }
}
