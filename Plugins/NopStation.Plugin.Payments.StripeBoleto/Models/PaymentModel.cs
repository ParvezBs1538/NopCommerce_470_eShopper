using System;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.StripeBoleto.Models
{
    public record PaymentModel : BaseNopEntityModel
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public string TaxId { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipPostalCode { get; set; }
        public string Country { get; set; }

        public string ClientSecret { get; set; }

        public string PublishableKey { get; set; }
    }
}
