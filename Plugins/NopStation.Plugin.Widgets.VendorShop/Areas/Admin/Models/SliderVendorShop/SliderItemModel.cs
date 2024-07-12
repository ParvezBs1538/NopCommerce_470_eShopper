using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.SliderVendorShop
{
    public record SliderItemModel : BaseNopEntityModel, ILocalizedModel<SliderItemLocalizedModel>
    {
        public SliderItemModel()
        {
            Locales = new List<SliderItemLocalizedModel>();
        }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Title")]
        public string SliderItemTitle { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.ShortDescription")]
        public string ShortDescription { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Picture")]
        public int PictureId { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.MobilePicture")]
        public int MobilePictureId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.ImageAltText")]
        public string ImageAltText { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Link")]
        public string Link { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Picture")]
        public string PictureUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.MobilePicture")]
        public string MobilePictureUrl { get; set; }

        public string FullPictureUrl { get; set; }

        public string MobileFullPictureUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public int SliderId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.ShopNowLink")]
        public string ShopNowLink { get; set; }

        public IList<SliderItemLocalizedModel> Locales { get; set; }
    }

    public class SliderItemLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Title")]
        public string SliderItemTitle { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.ShortDescription")]
        public string ShortDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.ImageAltText")]
        public string ImageAltText { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Link")]
        public string Link { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.ShopNowLink")]
        public string ShopNowLink { get; set; }
    }
}
