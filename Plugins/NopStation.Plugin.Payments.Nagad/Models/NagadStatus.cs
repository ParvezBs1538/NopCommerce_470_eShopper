namespace NopStation.Plugin.Payments.Nagad.Models
{
    public enum NagadStatus
    {
        Success,
        OrderInitiated,
        Ready,
        InProgress,
        OtpSent,
        OtpVerified,
        PinGiven,
        Cancelled,
        PartialCancelled,
        InvalidRequest,
        Fraud,
        Aborted,
        UnKnownFailed,
        Failed
    }
}
