using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.IpFilter.Models
{
    public record IpRangeBlockRuleSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.NopStation.IpFilter.IpRangeBlockRules.List.CreatedFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedFrom { get; set; }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.IpRangeBlockRules.List.CreatedTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedTo { get; set; }
    }
}
