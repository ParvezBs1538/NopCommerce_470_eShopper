using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.IpFilter.Models
{
    public record IpBlockRuleModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.IpFilter.IpBlockRules.Fields.IpAddress")]
        public string IpAddress { get; set; }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.IpBlockRules.Fields.Location")]
        public string Location { get; set; }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.IpBlockRules.Fields.Comment")]
        public string Comment { get; set; }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.IpBlockRules.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.IpFilter.IpBlockRules.Fields.IsAllowed")]
        public bool IsAllowed { get; set; }
    }
}
