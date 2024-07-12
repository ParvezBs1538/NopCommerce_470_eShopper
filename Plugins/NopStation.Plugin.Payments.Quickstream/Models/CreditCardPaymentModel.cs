namespace NopStation.Plugin.Payments.Quickstream.Models
{
    public class CreditCardPaymentModel
    {
        public string CardholderName { get; set; }

        public string CardNumber { get; set; }

        public string ExpireMonth { get; set; }

        public string ExpireYear { get; set; }

        public string CardCode { get; set; }
    }
}
