namespace NopStation.Plugin.Payments.Dmoney.Models
{
    public class PaymentPublicModel
    {
        public string GatewayUrl { get; set; }

        public string OrganizationCode { get; set; }

        public string Password { get; set; }

        public string SecretKey { get; set; }

        public string BillerCode { get; set; }

        public int OrderId { get; set; }

        public string TransactionTrackingNo { get; set; }

        public decimal OrderTotal { get; set; }

        public string ApproveUrl { get; set; }

        public string CancelUrl { get; set; }

        public string DeclineUrl { get; set; }

        public string Description { get; set; }
    }
}
