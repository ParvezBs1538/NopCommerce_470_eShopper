using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Models;

public record CarouselModel : BaseNopEntityModel
{
    public CarouselModel()
    {
        Products = new List<ProductOverviewModel>();
        Picture = new PictureModel();
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

    public bool ShowCarouselPicture { get; set; }

    public PictureModel Picture { get; set; }

    public PositionType PicturePosition { get; set; }

    public bool ShowCountdown { get; set; }

    public DateTime? CountdownUntill { get; set; }

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

    public IList<ProductOverviewModel> Products { get; set; }

    public BackgroundType BackgroundType { get; set; }
}
