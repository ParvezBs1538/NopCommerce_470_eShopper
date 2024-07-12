using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.MPay24.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string Description { get; set; }

        public bool Selected { get; set; }

        public string LogoUrl { get; set; }
    }
}
