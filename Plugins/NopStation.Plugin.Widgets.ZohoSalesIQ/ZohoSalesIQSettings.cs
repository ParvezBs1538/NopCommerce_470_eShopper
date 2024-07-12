using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.ZohoSalesIQ
{
    public class ZohoSalesIQSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string Script { get; set; }
    }
}