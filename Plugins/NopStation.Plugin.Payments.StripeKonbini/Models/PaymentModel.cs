﻿using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.StripeKonbini.Models
{
    public record PaymentModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string ConfirmationNumber { get; set; }

        public string ClientSecret { get; set; }

        public string PublishableKey { get; set; }
    }
}