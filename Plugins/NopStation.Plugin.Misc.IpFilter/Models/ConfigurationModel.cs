using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.IpFilter.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.NopStation.IpFilter.Configuration.Fields.IsEnabled")]
        public bool IsEnabled { get; set; }
    }
}