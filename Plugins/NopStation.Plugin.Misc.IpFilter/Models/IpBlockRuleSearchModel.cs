using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.IpFilter.Models
{
    public record IpBlockRuleSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.NopStation.IpFilter.IpBlockRules.List.CreatedFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedFrom { get; set; }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.IpBlockRules.List.CreatedTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedTo { get; set; }
    }
}
