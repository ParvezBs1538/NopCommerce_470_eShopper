using System.ComponentModel.DataAnnotations;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.Widgets.Popups.Models;

public record NewsletterPopupModel
{
    public NewsletterPopupModel()
    {
        BackgroundPicture = new PictureModel();
    }

    public bool PopupEnabled { get; set; }

    public string Title { get; set; }

    public string Desctiption { get; set; }

    public bool HasBackgroundPicture { get; set; }

    public PictureModel BackgroundPicture { get; set; }

    public string RedirectUrl { get; set; }

    public string PopupOpenerSelector { get; set; }

    public bool OpenPopupOnLoadPage { get; set; }

    public int DelayTime { get; set; }

    public bool AllowCustomerToSelectDoNotShowThisPopupAgain { get; set; }

    public bool PreSelectedDoNotShowThisPopupAgain { get; set; }

    [DataType(DataType.EmailAddress)]
    public string NewsletterEmail { get; set; }

    public bool AllowToUnsubscribe { get; set; }
}
