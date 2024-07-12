namespace NopStation.Plugin.Payout.Stripe
{
    public static class StripeDefaults
    {
        public static string SystemName => "Payout.Stripe";
        public static string CreatePayout => "https://api.stripe.com/v1/payouts";
        public static string CreateTransfer => "https://api.stripe.com/v1/transfers";
    }
}