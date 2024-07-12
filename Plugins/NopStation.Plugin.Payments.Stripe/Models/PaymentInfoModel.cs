using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.Stripe.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public PaymentInfoModel()
        {
            CreditCardTypes = new List<SelectListItem>();
            ExpireMonths = new List<SelectListItem>();
            ExpireYears = new List<SelectListItem>();
        }

        [NopResourceDisplayName("NopStation.Stripe.CreditCardType")]
        public string CreditCardType { get; set; }

        public IList<SelectListItem> CreditCardTypes { get; set; }

        [NopResourceDisplayName("NopStation.Stripe.CardholderName")]
        public string CardholderName { get; set; }

        [NopResourceDisplayName("NopStation.Stripe.CardNumber")]
        public string CardNumber { get; set; }

        [NopResourceDisplayName("NopStation.Stripe.ExpirationDate")]
        public string ExpireMonth { get; set; }

        [NopResourceDisplayName("NopStation.Stripe.ExpirationDate")]
        public string ExpireYear { get; set; }
        public IList<SelectListItem> ExpireMonths { get; set; }
        public IList<SelectListItem> ExpireYears { get; set; }

        [NopResourceDisplayName("NopStation.Stripe.CardCode")]
        public string CardCode { get; set; }
    }
}
