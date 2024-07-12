namespace NopStation.Plugin.Payments.CreditWallet.Domain
{
    public enum ActivityType
    {
        OrderPlaced = 1,
        OrderCancelled = 2,
        AdminCreate = 3,
        AdminModify = 4,
        InvoicePayment = 5,
        InvoiceAdjustment = 6
    }
}
