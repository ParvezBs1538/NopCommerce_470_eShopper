using System.Collections.Generic;

namespace NopStation.Plugin.Payments.Dmoney.Domains
{
    public class VerifyTransactionResult
    {
        public VerifyTransactionResult()
        {
            Errors = new List<string>();
        }

        public bool Status { get; set; }

        public int OrderId { get; set; }

        public void AddError(string error)
        {
            this.Errors.Add(error);
        }

        public IList<string> Errors { get; set; }
    }
}
