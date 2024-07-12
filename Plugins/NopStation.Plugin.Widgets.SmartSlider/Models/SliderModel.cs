using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Models;

public record SliderModel : BaseNopEntityModel
{
    public SliderModel()
    {
        SliderItems = new List<SliderItemModel>();
    }

    public bool ShowBackground { get; set; }

    public string BackgroundPictureUrl { get; set; }

    public string BackgroundColor { get; set; }

    public bool AutoPlay { get; set; }

    public string CustomCssClass { get; set; }

    public bool Loop { get; set; }

    public int StartPosition { get; set; }

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

    public bool Zoom { get; set; }

    public decimal ZoomMaximumRatio { get; set; }

    public decimal ZoomMinimumRatio { get; set; }

    public bool ToggleZoom { get; set; }

    public bool Effect { get; set; }

    public EffectType EffectType { get; set; }

    public bool MousewheelControl { get; set; }

    public bool MousewheelControlForceToAxis { get; set; }

    public bool VerticalDirection { get; set; }

    public bool AutoHeight { get; set; }

    public bool AllowTouchMove { get; set; }

    public PaginationType PaginationType { get; set; }

    public BackgroundType BackgroundType { get; set; }

    public IList<SliderItemModel> SliderItems { get; set; }


    public record SliderItemModel : BaseNopEntityModel
    {
        public SliderItemModel()
        {
            Picture = new PictureModel();
        }

        public int SliderId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string RedirectUrl { get; set; }

        public string ButtonText { get; set; }

        public bool ShowCaption { get; set; }

        public PictureModel Picture { get; set; }

        public string SliderVideoUrl { get; set; }

        public string EmbeddedLink { get; set; }

        public string Text { get; set; }

        public ContentType ContentType { get; set; }
    }
}
