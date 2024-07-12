using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.Quickstream.Models
{
    public record PaymentInfoViewModel : BaseNopModel
    {
        public PaymentInfoViewModel()
        {
            PaymentTypes = new List<SelectListItem>();
            ExpireMonths = new List<SelectListItem>();
            ExpireYears = new List<SelectListItem>();
            AcceptCardUrls = new List<string>();
        }

        [NopResourceDisplayName("NopStation.QuickStream.PaymentInfo.Fields.SelectCreditCard")]
        public string PaymentType { get; set; }

        [NopResourceDisplayName("NopStation.QuickStream.PaymentInfo.Fields.SelectCreditCard")]
        public IList<SelectListItem> PaymentTypes { get; set; }

        [NopResourceDisplayName("NopStation.QuickStream.PaymentInfo.Fields.CardholderName")]
        public string CardholderName { get; set; }

        [NopResourceDisplayName("NopStation.QuickStream.PaymentInfo.Fields.CardNumber")]
        public string CardNumber { get; set; }

        [NopResourceDisplayName("NopStation.QuickStream.PaymentInfo.Fields.ExpirationDate")]
        public string ExpireMonth { get; set; }

        [NopResourceDisplayName("NopStation.QuickStream.PaymentInfo.Fields.ExpirationDate")]
        public string ExpireYear { get; set; }

        public IList<SelectListItem> ExpireMonths { get; set; }

        public IList<SelectListItem> ExpireYears { get; set; }

        [NopResourceDisplayName("NopStation.QuickStream.PaymentInfo.Fields.CardCode")]
        public string CardCode { get; set; }

        [NopResourceDisplayName("NopStation.QuickStream.PaymentInfo.Fields.AccountName")]
        public string AccountName { get; set; }

        [NopResourceDisplayName("NopStation.QuickStream.PaymentInfo.Fields.BSB")]
        public string Bsb { get; set; }

        [NopResourceDisplayName("NopStation.QuickStream.PaymentInfo.Fields.AccountNumber")]
        public string AccountNumber { get; set; }

        public string OrderReference { get; set; }

        public List<string> AcceptCardUrls { get; set; }
    }
}
