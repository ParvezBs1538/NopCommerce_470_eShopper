using System;

namespace NopStation.Plugin.Misc.AutoCancelOrder.Services
{
    public class SearchParam
    {
        public string PaymentMethodSystemName { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}
