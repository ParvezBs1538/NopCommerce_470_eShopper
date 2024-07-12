using System;

namespace NopStation.Plugin.Payments.SSLCommerz.Sevices.Results
{
    public class RefundValidationResult
    {
        public DateTime? InitiatedOn { get; set; }

        public DateTime? RefundedOn { get; set; }

        public bool Success { get; set; }
    }
}
