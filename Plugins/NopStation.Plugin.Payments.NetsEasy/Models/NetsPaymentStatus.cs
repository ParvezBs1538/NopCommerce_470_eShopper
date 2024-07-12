using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.NetsEasy.Models
{
    public enum NetsPaymentStatus
    {
        Succeeded = 10,
        Failed = 20,
        Pending = 20,
    }
}
