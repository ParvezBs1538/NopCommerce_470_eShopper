using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models
{
    public record StorePickupPointModel : BaseNopEntityModel
    {
        public StorePickupPointModel()
        {
            Address = new AddressModel();
            AvailableStores = new List<SelectListItem>();
        }

        public AddressModel Address { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Fields.Name")]
        public string Name { get; set; }
        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Fields.Active")]
        public bool Active { get; set; }
        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Fields.PickupFee")]
        public decimal PickupFee { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Fields.OpeningHours")]
        public string OpeningHours { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public List<SelectListItem> AvailableStores { get; set; }
        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Fields.Store")]
        public int StoreId { get; set; }
        public string StoreName { get; set; }

        [DataType(DataType.Text)]
        [DisplayFormat(DataFormatString = "{0:F8}", ApplyFormatInEditMode = true)]
        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Fields.Latitude")]
        public decimal? Latitude { get; set; }

        [DataType(DataType.Text)]
        [DisplayFormat(DataFormatString = "{0:F8}", ApplyFormatInEditMode = true)]
        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Fields.Longitude")]
        public decimal? Longitude { get; set; }

        [UIHint("Int32Nullable")]
        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Fields.TransitDays")]
        public int? TransitDays { get; set; }
    }

    public class AddressModel
    {
        public AddressModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Address.Fields.Country")]
        public int? CountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
        public bool CountryEnabled { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.StateProvince")]
        public int? StateProvinceId { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public bool StateProvinceEnabled { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.County")]
        public string County { get; set; }
        public bool CountyEnabled { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.City")]
        public string City { get; set; }
        public bool CityEnabled { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.Address1")]
        public string Address1 { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }
        public bool ZipPostalCodeEnabled { get; set; }
    }
}