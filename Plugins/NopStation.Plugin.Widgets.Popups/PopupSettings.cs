using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.Popups;

public class PopupSettings : ISettings
{
    public bool EnableNewsletterPopup { get; set; }

    public string NewsletterPopupTitle { get; set; }

    public string NewsletterPopupDesctiption { get; set; }

    public int BackgroundPictureId { get; set; }

    public string RedirectUrl { get; set; }

    public string PopupOpenerSelector { get; set; }

    public bool OpenPopupOnLoadPage { get; set; }

    public int DelayTime { get; set; }

    public bool AllowCustomerToSelectDoNotShowThisPopupAgain { get; set; }

    public bool PreSelectedDoNotShowThisPopupAgain { get; set; }

    public bool ShowOnHomePageOnly { get; set; }
}