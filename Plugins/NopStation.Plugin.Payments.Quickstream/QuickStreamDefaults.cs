namespace NopStation.Plugin.Payments.Quickstream
{
    public static class QuickStreamDefaults
    {
        public static string PLUGIN_SYSTEM_NAME => "NopStation.Plugin.Payments.Quickstream";
        public static string PAYMENT_STATUS_UPDATE => "NopStation.Plugin.Payments.Quickstream.UpdatePaymentStatusTask";
        public static string SANDBOX_SINGLE_USE_TOKEN_URL => "https://api.quickstream.support.qvalent.com/rest/v1/single-use-tokens";
        public static string SINGLE_USE_TOKEN_URL => "https://api.quickstream.westpac.com.au/rest/v1/single-use-tokens";
        public static string TAKE_PAYMENT_URL => "https://api.quickstream.westpac.com.au/rest/v1/transactions";
        public static string SANDBOX_TAKE_PAYMENT_URL => "https://api.quickstream.support.qvalent.com/rest/v1/transactions";

        public static string GET_TRANSACTION_URL => "https://api.quickstream.westpac.com.au/rest/v1/transactions/{0}";
        public static string SANDBOX_GET_TRANSACTION_URL => "https://api.quickstream.support.qvalent.com/rest/v1/transactions/{0}";

        public static string REFUND_URL => "https://api.quickstream.westpac.com.au/rest/v1/transactions";
        public static string SANDBOX_REFUND_URL => "https://api.quickstream.support.qvalent.com/rest/v1/transactions";

        public static string QUERY_CARD_SURCHARGE_URL => "https://api.quickstream.westpac.com.au/rest/v1/businesses/{0}/query-card-surcharge";
        public static string SANDBOX_QUERY_CARD_SURCHARGE_URL => "https://api.quickstream.support.qvalent.com/rest/v1/businesses/{0}/query-card-surcharge";

        public static string CARDS_SURCHARGE_URL => "https://api.quickstream.westpac.com.au/rest/v1/businesses/{0}/card-surcharges";
        public static string SANDBOX_CARDS_SURCHARGE_URL => "https://api.quickstream.support.qvalent.com/rest/v1/businesses/{0}/card-surcharges";

        public static string TRANSACTION_TYPE_PAYMENT => "PAYMENT";
        public static string TRANSACTION_TYPE_REFUND => "REFUND";
        public static string EC_INTERNET => "INTERNET";
        public static string ErrorMail => "abc@mail.com";
        public static string APPROVED => "0";
        public static string NOT_APPROVED_YET => "2";
    }
}
