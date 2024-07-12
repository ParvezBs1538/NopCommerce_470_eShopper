using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

public class MegaMenuItem : BaseEntity, ILocalizedEntity, IAclSupported
{
    public string Title { get; set; }

    public string Url { get; set; }

    public int MegaMenuId { get; set; }

    public bool OpenInNewTab { get; set; }

    public string CssClass { get; set; }

    public bool ShowRibbonText { get; set; }

    public string RibbonText { get; set; }

    public string RibbonBackgroundColor { get; set; }

    public string RibbonTextColor { get; set; }

    public int ParentMenuItemId { get; set; }

    public int CategoryId { get; set; }

    public int ManufacturerId { get; set; }

    public int VendorId { get; set; }

    public int TopicId { get; set; }

    public int ProductTagId { get; set; }

    public int PageTypeId { get; set; }

    public int MenuItemTypeId { get; set; }

    public bool ShowPicture { get; set; }

    public int PictureId { get; set; }

    public bool SubjectToAcl { get; set; }

    public int DisplayOrder { get; set; }

    public MenuItemType MenuItemType
    {
        get => (MenuItemType)MenuItemTypeId;
        set => MenuItemTypeId = (int)value;
    }

    public PageType PageType
    {
        get => (PageType)PageTypeId;
        set => PageTypeId = (int)value;
    }
}
