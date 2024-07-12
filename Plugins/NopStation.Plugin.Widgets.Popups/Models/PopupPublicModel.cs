using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.Popups.Models;

public record PopupPublicModel
{
    public PopupPublicModel()
    {
        Popups = new List<PopupModel>();

        NewsletterPopup = new NewsletterPopupModel();
    }

    public IList<PopupModel> Popups { get; set; }

    public NewsletterPopupModel NewsletterPopup { get; set; }
}
