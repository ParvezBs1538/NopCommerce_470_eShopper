using System.Collections.Generic;
using System.Linq;

namespace NopStation.Plugin.Payments.SSLCommerz.Sevices.Results
{
    public class PaymentInitResult
    {
        public PaymentInitResult()
        {
            Errors = new List<string>();
        }

        public string RedirectUrl { get; set; }

        public bool Success => !Errors.Any();

        public IList<string> Errors { get; set; }
    }
}
