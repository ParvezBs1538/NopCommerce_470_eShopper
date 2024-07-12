using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.OCarouselVendorShop
{
    public record OCarouselModel : BaseNopEntityModel, ILocalizedModel<OCarouselLocalizedModel>, IStoreMappingSupportedModel
    {
        public OCarouselModel()
        {
            AvailableDataSources = new List<SelectListItem>();
            AvailableWidgetZones = new List<SelectListItem>();
            OCarouselItemSearchModel = new OCarouselItemSearchModel();
            Locales = new List<OCarouselLocalizedModel>();
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
            NumberOfItemsToShow = 10;
        }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Name")]
        public string Name { get; set; }
        public int VendorId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.DisplayTitle")]
        public bool DisplayTitle { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.WidgetZone")]
        public int WidgetZoneId { get; set; }
        public string WidgetZoneStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.DataSourceType")]
        public int DataSourceTypeId { get; set; }
        public string DataSourceTypeStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.ShowBackgroundPicture")]
        public bool ShowBackgroundPicture { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.BackgroundPicture")]
        [UIHint("Picture")]
        public int BackgroundPictureId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.CustomUrl")]
        public string CustomUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.NumberOfItemsToShow")]
        public int NumberOfItemsToShow { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.AutoPlay")]
        public bool AutoPlay { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.CustomCssClass")]
        public string CustomCssClass { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Loop")]
        public bool Loop { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.StartPosition")]
        public int StartPosition { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Center")]
        public bool Center { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Nav")]
        public bool Nav { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.LazyLoad")]
        public bool LazyLoad { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.LazyLoadEager")]
        public int LazyLoadEager { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.AutoPlayTimeout")]
        public int AutoPlayTimeout { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.AutoPlayHoverPause")]
        public bool AutoPlayHoverPause { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }

        public OCarouselItemSearchModel OCarouselItemSearchModel { get; set; }

        public IList<SelectListItem> AvailableWidgetZones { get; set; }
        public IList<SelectListItem> AvailableDataSources { get; set; }

        public IList<OCarouselLocalizedModel> Locales { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.SelectedStoreIds")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }

    public class OCarouselLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Title")]
        public string Title { get; set; }
    }
}