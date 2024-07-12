namespace NopStation.Plugin.Payments.OABIPayment.Models
{
    public class ResponseData
    {
        public string Status { get; set; }
        public string TrackId { get; set; }
        public string Amount { get; set; }
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string PaymentId { get; set; }
    }
}
