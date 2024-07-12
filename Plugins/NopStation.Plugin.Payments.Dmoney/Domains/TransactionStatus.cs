namespace NopStation.Plugin.Payments.Dmoney.Domains
{
    public enum TransactionStatus
    {
        Init = 0,

        Open = 5,

        Success = 10,

        Failed = 15,

        Declined = 20
    }
}
