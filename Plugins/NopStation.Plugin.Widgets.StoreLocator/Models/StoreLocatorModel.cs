using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NopStation.Plugin.Widgets.StoreLocator.Models
{
    public record StoreLocatorModel
    {
        public StoreLocatorModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            LocationsModel = new LocationsModel();
        }

        public int CountryId { get; set; }

        public string GoogleMapApiKey { get; set; }

        public int StateProvinceId { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }

        public IList<SelectListItem> AvailableStates { get; set; }

        public LocationsModel LocationsModel { get; set; }
    }
}
