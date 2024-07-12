namespace NopStation.Plugin.Payments.CBL
{
    public static class CBLPaymentDefaults
    {
        public static string PLUGIN_SYSTEM_NAME => "NopStation.Plugin.Payments.CBL";
        public static string SANDBOX_BASE_URL => "https://sandbox.thecitybank.com";
        public static string BASE_URL => "https://thecitybank.com";
        public static string TRANSACTION_Port => ":8011";
        public static string TRANSACTION_URL_RESOURCE => "/transaction/tdst/tokenwpcidss";
        public static string TRANSACTION_DETAILS_URL => "/transaction/getorderdetailsapiwpcidss";
        public static string RedirectUrlKey => "Order.CBL.RedirectUrl";
    }
}
