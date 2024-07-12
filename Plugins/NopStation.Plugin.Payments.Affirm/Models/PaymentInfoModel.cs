using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.Affirm.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public PaymentInfoModel()
        {
            Warnings = new List<string>();
        }

        public IList<string> Warnings { get; set; }

        public string AffirmJSON { get; set; }

        public string JsURL { get; set; }

        public string PublicApiKey { get; set; }
    }
}
