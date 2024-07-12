using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.ProductTabs
{
    public record ProductTabModel : BaseNopEntityModel, ILocalizedModel<ProductTabLocalizedModel>, IStoreMappingSupportedModel
    {
        public ProductTabModel()
        {
            AvailableWidgetZones = new List<SelectListItem>();
            ProductTabItemSearchModel = new ProductTabItemSearchModel();
            Locales = new List<ProductTabLocalizedModel>();
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Title")]
        public string TabTitle { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.DisplayTitle")]
        public bool DisplayTitle { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Picture")]
        [UIHint("Picture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.PictureAlt")]
        public string PictureAlt { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.PictureTitle")]
        public string PictureTitle { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.CustomUrl")]
        public string CustomUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.WidgetZone")]
        public int WidgetZoneId { get; set; }
        public string WidgetZoneStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.AutoPlay")]
        public bool AutoPlay { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.CustomCssClass")]
        public string CustomCssClass { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Loop")]
        public bool Loop { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Margin")]
        public int Margin { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.StartPosition")]
        public int StartPosition { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Center")]
        public bool Center { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Nav")]
        public bool Nav { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.LazyLoad")]
        public bool LazyLoad { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.LazyLoadEager")]
        public int LazyLoadEager { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.AutoPlayTimeout")]
        public int AutoPlayTimeout { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.AutoPlayHoverPause")]
        public bool AutoPlayHoverPause { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }


        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.SelectedStoreIds")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        public ProductTabItemSearchModel ProductTabItemSearchModel { get; set; }

        public IList<SelectListItem> AvailableWidgetZones { get; set; }

        public IList<ProductTabLocalizedModel> Locales { get; set; }
    }

    public class ProductTabLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Title")]
        public string TabTitle { get; set; }
    }
}
