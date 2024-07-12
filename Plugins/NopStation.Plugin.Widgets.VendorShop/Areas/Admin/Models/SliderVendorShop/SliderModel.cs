using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.SliderVendorShop
{
    public record SliderModel : BaseNopEntityModel, ILocalizedModel<SliderLocalizedModel>, IStoreMappingSupportedModel
    {
        public SliderModel()
        {
            AvailableWidgetZones = new List<SelectListItem>();
            AvailableAnimationTypes = new List<SelectListItem>();
            SliderItemSearchModel = new SliderItemSearchModel();
            SelectedStoreIds = new List<int>();
            Locales = new List<SliderLocalizedModel>();
        }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Name")]
        public string Name { get; set; }
        public int VendorId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.WidgetZone")]
        public int WidgetZoneId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.WidgetZone")]
        public string WidgetZoneStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.ShowBackgroundPicture")]
        public bool ShowBackgroundPicture { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.BackGroundPicture")]
        public int BackgroundPictureId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Nav")]
        public bool Nav { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AutoPlay")]
        public bool AutoPlay { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AutoPlayTimeout")]
        public int AutoPlayTimeout { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AutoPlayHoverPause")]
        public bool AutoPlayHoverPause { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.StartPosition")]
        public int StartPosition { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.LazyLoad")]
        public bool LazyLoad { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.LazyLoadEager")]
        public int LazyLoadEager { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Video")]
        public bool Video { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Loop")]
        public bool Loop { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Margin")]
        public int Margin { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AnimateOut")]
        public string AnimateOut { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AnimateIn")]
        public string AnimateIn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.SelectedStoreIds")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableWidgetZones { get; set; }

        public IList<SelectListItem> AvailableAnimationTypes { get; set; }

        public SliderItemSearchModel SliderItemSearchModel { get; set; }

        public IList<SliderLocalizedModel> Locales { get; set; }
    }

    public class SliderLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Name")]
        public string Name { get; set; }
    }
}
