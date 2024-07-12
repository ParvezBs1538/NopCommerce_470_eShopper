namespace NopStation.Plugin.Payout.PayPal
{
    public static class PayPalDefaults
    {
        public static string SystemName => "Payout.PayPal";
        public static string WebHookEventType => "PAYMENT.PAYOUTS-ITEM.SUCCEEDED";
    }
}