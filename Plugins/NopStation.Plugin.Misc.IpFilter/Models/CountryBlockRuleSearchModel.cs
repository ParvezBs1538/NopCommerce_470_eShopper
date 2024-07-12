using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.IpFilter.Models
{
    public record CountryBlockRuleSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.NopStation.IpFilter.CountryBlockRules.List.CreatedFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedFrom { get; set; }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.CountryBlockRules.List.CreatedTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedTo { get; set; }
    }
}
