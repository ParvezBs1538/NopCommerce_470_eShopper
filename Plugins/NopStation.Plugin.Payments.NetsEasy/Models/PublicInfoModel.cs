using System.Collections.Generic;
using System.Linq;

namespace NopStation.Plugin.Payments.NetsEasy.Models
{
    public class PublicInfoModel
    {
        public PublicInfoModel()
        {
            Errors = new List<string>();
        }

        public string PaymentId { get; set; }

        public string CheckoutKey { get; set; }

        public string CheckoutScriptUrl { get; set; }

        public string CurrentLanguage { get; set; }

        public IList<string> Errors { get; set; }

        public bool Success => !Errors.Any();

        public void AddError(string error)
        {
            Errors.Add(error);
        }
    }
}
