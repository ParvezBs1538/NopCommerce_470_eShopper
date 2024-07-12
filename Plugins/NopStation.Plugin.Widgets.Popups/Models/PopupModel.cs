using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.Popups.Domains;

namespace NopStation.Plugin.Widgets.Popups.Models;

public record PopupModel : BaseNopEntityModel
{
    public PopupModel()
    {
        Column1 = new ColumnModel();
        Column2 = new ColumnModel();
    }

    public string Name { get; set; }

    public ColumnModel Column1 { get; set; }

    public ColumnModel Column2 { get; set; }

    public bool EnableStickyButton { get; set; }

    public string StickyButtonColor { get; set; }

    public string StickyButtonText { get; set; }

    public bool OpenPopupOnLoadPage { get; set; }

    public int DelayTime { get; set; }

    public bool AllowCustomerToSelectDoNotShowThisPopupAgain { get; set; }

    public bool PreSelectedDoNotShowThisPopupAgain { get; set; }

    public string CssClass { get; set; }

    public Position StickyButtonPosition { get; set; }

    public ColumnType ColumnType { get; set; }

    public class ColumnModel
    {
        public ColumnModel()
        {
            Picture = new PictureModel();
        }

        public string Text { get; set; }

        public PictureModel Picture { get; set; }

        public string PopupUrl { get; set; }

        public ContentType ContentType { get; set; }
    }
}
