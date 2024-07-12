using System.Collections.Generic;
using NopStation.Plugin.Misc.AjaxFilter.Domains.Enums;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class FilterVendorsModel
    {
        public FilterVendorsModel()
        {
            Vendors = new List<VendorsModel>();
        }

        public FiltersUI CheckOrDropdown { get; set; }

        public IList<VendorsModel> Vendors { get; set; }

    }
}
