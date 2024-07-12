using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payout.PayPal.Areas.Admin.Models
{
    public record VendorPayPalConfigurationModel : BaseNopModel
    {
        public int VendorId { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.Payout.PayPal.Configuration.Fields.PayPalEmail")]
        public string PayPalEmail { get; set; }
    }
}
