using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.ProductPdf
{
    public class ProductPdfSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public bool LetterPageSizeEnabled { get; set; }
    }
}
