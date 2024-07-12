using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Models
{
    public record StoreLocationModel : BaseNopEntityModel, ILocalizedModel<StoreLocationLocalizedModel>, IStoreMappingSupportedModel
    {
        public StoreLocationModel()
        {
            Locales = new List<StoreLocationLocalizedModel>();
            Address = new AddressModel();

            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
            StoreLocationPictureSearchModel = new StoreLocationPictureSearchModel();
            AddPictureModel = new StoreLocationPictureModel();
        }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.OpeningHours")]
        public string OpeningHours { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.Latitude")]
        public string Latitude { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.Longitude")]
        public string Longitude { get; set; }

        public int AddressId { get; set; }

        public AddressModel Address { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.ShortDescription")]
        public string ShortDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.FullDescription")]
        public string FullDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.GoogleMapLocation")]
        public string GoogleMapLocation { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.PickupFee")]
        public decimal PickupFee { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.IsPickupPoint")]
        public bool IsPickupPoint { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.FullAddress")]
        public string FullAddress { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.SeName")]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<StoreLocationLocalizedModel> Locales { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.Picture")]
        public string PictureUrl { get; set; }

        public StoreLocationPictureSearchModel StoreLocationPictureSearchModel { get; set; }
        public StoreLocationPictureModel AddPictureModel { get; set; }
    }

    public partial record StoreLocationLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.ShortDescription")]
        public string ShortDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.FullDescription")]
        public string FullDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.OpeningHours")]
        public string OpeningHours { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Fields.SeName")]
        public string SeName { get; set; }
    }
}
