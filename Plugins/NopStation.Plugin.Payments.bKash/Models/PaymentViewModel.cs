namespace NopStation.Plugin.Payments.bKash.Models
{
    public class PaymentViewModel
    {
        public int OrderId { get; set; }
        public decimal OrderTotal { get; set; }
        public string Intent { get; set; }
        public bool IsSandBox { get; set; }
        public string CustomOrderNumber { get; set; }
    }
}
