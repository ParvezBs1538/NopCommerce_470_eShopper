using System.Collections.Generic;
using NopStation.Plugin.Misc.AjaxFilter.Domains.Enums;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class FilterManufacturersModel
    {
        public FilterManufacturersModel()
        {
            Manufacturers = new List<ManufacturersModel>();
        }

        public FiltersUI CheckOrDropdown { get; set; }

        public int MaxManufacturersToDisplay { get; set; }
        public IList<ManufacturersModel> Manufacturers { get; set; }
    }
}
