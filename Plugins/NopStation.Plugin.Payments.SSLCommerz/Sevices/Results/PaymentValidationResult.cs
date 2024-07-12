using System.Collections.Generic;
using System.Linq;

namespace NopStation.Plugin.Payments.SSLCommerz.Sevices.Results
{
    public class PaymentValidationResult
    {
        public PaymentValidationResult()
        {
            Errors = new List<string>();
        }

        public bool Success => !Errors.Any();

        public IList<string> Errors { get; set; }
    }
}
