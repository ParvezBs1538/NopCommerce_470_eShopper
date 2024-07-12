using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models
{
    public class RefundResponse
    {
        public RefundResponse()
        {
            Errors = new List<ErrorList>();
        }

        [JsonProperty("refundTransactionId")]
        public string RefundTransactionId { get; set; }

        [JsonProperty("message")]
        public List<ErrorList> Errors { get; set; }
    }
    public class ErrorList
    {
        [JsonProperty("errorName")]
        public string ErrorName { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
