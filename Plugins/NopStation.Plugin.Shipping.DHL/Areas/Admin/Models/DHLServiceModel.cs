using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Shipping.DHL.Areas.Admin.Models
{
    public record DHLServiceModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.DHL.Services.Fields.ServiceName")]
        public string ServiceName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Services.Fields.GlobalProductCode")]
        public string GlobalProductCode { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Services.Fields.IsActive")]
        public bool IsActive { get; set; }
    }
}
