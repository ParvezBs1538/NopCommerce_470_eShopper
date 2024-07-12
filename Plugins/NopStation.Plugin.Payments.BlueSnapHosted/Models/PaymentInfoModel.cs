using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public bool IsSandBox { get; set; }

        public string Token { set; get; }

        [NopResourceDisplayName("NopStation.BlueSnapHosted.Fields.FullName")]
        public string FullName { set; get; }
        public string FirstName { set; get; }
        public string LastName { set; get; }

        [NopResourceDisplayName("NopStation.BlueSnapHosted.Fields.CardNumber")]
        public string CardNumber { get; set; }

        [NopResourceDisplayName("NopStation.BlueSnapHosted.Fields.ExpirationDate")]
        public string ExpireMonth { get; set; }

        [NopResourceDisplayName("NopStation.BlueSnapHosted.Fields.CardCode")]
        public string CardCode { get; set; }

        public string Zip { set; get; }
    }
}