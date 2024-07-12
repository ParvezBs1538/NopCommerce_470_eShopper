using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.IpFilter.Models
{
    public record CountryBlockRuleModel : BaseNopEntityModel
    {
        public CountryBlockRuleModel()
        {
            AvailableCountries = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.CountryBlockRules.Fields.Country")]
        public int CountryId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.CountryBlockRules.Fields.Country")]
        public string CountryName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.CountryBlockRules.Fields.Comment")]
        public string Comment { get; set; }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.CountryBlockRules.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
    }
}
