namespace NopStation.Plugin.Payments.Affirm.Models
{
    public class PromotionalPaymentInfoModel
    {
        public string PublicApiKey { get; set; }

        public string JsURL { get; set; }

        public string SystemName { get; set; }

        public int ProductId { get; set; }

        public int Amount { get; set; }
    }
}
