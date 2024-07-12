using System;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.AmazonPay.Models
{
    public record RedirectModel : BaseNopModel
    {
        public Guid OrderGuid { get; set; }

        public string ButtonColor { get; set; }

        public string ButtonType { get; set; }

        public string Payload { get; set; }

        public string PublicKeyId { get; set; }

        public string MerchantId { get; set; }

        public string AmazonSignatureAlgorithm { get; set; }

        public string Signature { get; set; }

        public string Currency { get; set; }

        public bool Sandbox { get; set; }

        public string PaymentScript { get; set; }
    }
}
