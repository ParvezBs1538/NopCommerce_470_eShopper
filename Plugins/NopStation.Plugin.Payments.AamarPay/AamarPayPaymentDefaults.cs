namespace NopStation.Plugin.Payments.AamarPay;

public static class AamarPayPaymentDefaults
{
    public static string PLUGIN_SYSTEM_NAME => "NopStation.Plugin.Payments.AamarPay";
    public static string SANDBOX_BASE_URL => "https://sandbox.aamarpay.com";
    public static string LIVE_BASE_URL => "https://secure.aamarpay.com";
    public static string PAYMENT_INIT_PATH => "/jsonpost.php";
    public static string SEARCH_TRANSACTION_PATH => "/api/v1/trxcheck/request.php?request_id={0}&store_id={1}&signature_key={2}&type=json";
}