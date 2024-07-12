namespace NopStation.Plugin.Payments.POLiPay.Models
{
    public class PaymentUrlRequest
    {
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string MerchantReference { get; set; }
        public string MerchantHomepageURL { get; set; }
        public string SuccessURL { get; set; }
        public string FailureURL { get; set; }
        public string CancellationURL { get; set; }
        public string NotificationURL { get; set; }
    }
}
