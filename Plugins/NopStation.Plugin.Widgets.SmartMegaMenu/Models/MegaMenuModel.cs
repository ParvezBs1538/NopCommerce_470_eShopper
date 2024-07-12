using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Models;

public record MegaMenuModel : BaseNopEntityModel
{
    public MegaMenuModel()
    {
        MenuItems = new List<MenuTreeItemModel>();
    }

    public string Name { get; set; }

    public int DisplayOrder { get; set; }

    public ViewType ViewType { get; set; }

    public bool WithoutImages { get; set; }

    public string CssClass { get; set; }

    public IList<MenuTreeItemModel> MenuItems { get; set; }

    public record MenuTreeItemModel : BaseNopEntityModel
    {
        public MenuTreeItemModel()
        {
            Children = new List<MenuTreeItemModel>();
            PictureModel = new PictureModel();
        }

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

        public string RouteName { get; set; }

        public object RouteParameter { get; set; }

        public string SeName { get; set; }

        public PageType PageType { get; set; }

        public MenuItemType MenuItemType { get; set; }

        public bool ShowPicture { get; set; }

        public PictureModel PictureModel { get; set; }

        public List<MenuTreeItemModel> Children { get; set; }
    }
}
