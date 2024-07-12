using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models
{
    public class PaymentResponse
    {
        public PaymentResponse()
        {
           Errors = new List<ErrorsList>();
           ProcessingInfo = new ProcessingInfo();
        }

        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("message")]
        public List<ErrorsList> Errors { get; set; }

        [JsonProperty("processingInfo")]
        public ProcessingInfo ProcessingInfo { get; set; }
    }

    public class ErrorsList
    {
        [JsonProperty("errorName")]
        public string ErrorName { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class ProcessingInfo
    {
        [JsonProperty("processingStatus")]
        public string ProcessingStatus { get; set; }
    }
}
