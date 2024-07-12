using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.PopupLogin
{
    public class PopupLoginSettings : ISettings
    {
        public bool EnablePopupLogin { get; set; }

        public string LoginUrlElementSelector { get; set; }
    }
}