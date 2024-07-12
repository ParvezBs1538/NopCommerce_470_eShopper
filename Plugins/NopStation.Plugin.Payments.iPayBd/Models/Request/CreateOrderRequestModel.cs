using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.iPayBd.Models.Request
{
    public class CreateOrderRequestModel
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("referenceId")]
        public string ReferenceId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("successCallbackUrl")]
        public string SuccessCallbackUrl { get; set; }

        [JsonProperty("failureCallbackUrl")]
        public string FailureCallbackUrl { get; set; }

        [JsonProperty("cancelCallbackUrl")]
        public string CancelCallbackUrl { get; set; }
    }
}
