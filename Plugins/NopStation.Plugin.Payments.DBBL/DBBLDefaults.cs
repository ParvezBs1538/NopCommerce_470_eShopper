namespace NopStation.Plugin.Payments.DBBL
{
    public static class DBBLDefaults
    {
        public static string SystemName = "NopStation.Plugin.Payments.DBBL";
        public static string CardType = "Card type";
        public static string TransactionOrder = "Transaction-{0}";

        public static string TestGateWayUrl = "https://ecomtest.dutchbanglabank.com/ecomm2/ClientHandler?card_type={0}&trans_id={1}";
        public static string LiveGateWayUrl = "https://ecom.dutchbanglabank.com/ecomm2/ClientHandler?card_type={0}&trans_id={1}";

        //public static string SelectedCardType = "PaymentMethod.DBBL.CardTypeValue.OrderGuid{0}";
        //public static string ProcessPaymentTxnRefNum = "PaymentMethod.DBBL.ProcessPaymentTxnRefNum";
    }
}
