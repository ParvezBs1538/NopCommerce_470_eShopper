namespace NopStation.Plugin.Payments.POLiPay.Models
{
    public class PaymentUrlResponse
    {
        public bool Success { get; set; }
        public string NavigateURL { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string TransactionRefNo { get; set; }
    }
}
