using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.FAQ
{
    public class FAQSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public bool IncludeInTopMenu { get; set; }

        public bool IncludeInFooter { get; set; }

        public string FooterElementSelector { get; set; }
    }
}
