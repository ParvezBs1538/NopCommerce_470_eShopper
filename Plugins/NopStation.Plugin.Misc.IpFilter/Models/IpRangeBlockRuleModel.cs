using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.IpFilter.Models
{
    public record IpRangeBlockRuleModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.FromIpAddress")]
        public string FromIpAddress { get; set; }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.ToIpAddress")]
        public string ToIpAddress { get; set; }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.Comment")]
        public string Comment { get; set; }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}
