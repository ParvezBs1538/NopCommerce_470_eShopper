using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;

public record MegaMenuItemModel : BaseNopEntityModel, ILocalizedModel<MegaMenuItemLocalizedModel>, IAclSupportedModel
{
    public MegaMenuItemModel()
    {
        Locales = new List<MegaMenuItemLocalizedModel>();

        SelectedCustomerRoleIds = new List<int>();
        AvailableCustomerRoles = new List<SelectListItem>();
    }

    public int MegaMenuId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Title")]
    public string Title { get; set; }

    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Url")]
    public string Url { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.OpenInNewTab")]
    public bool OpenInNewTab { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.CssClass")]
    public string CssClass { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.ShowRibbonText")]
    public bool ShowRibbonText { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.RibbonText")]
    public string RibbonText { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.RibbonBackgroundColor")]
    public string RibbonBackgroundColor { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.RibbonTextColor")]
    public string RibbonTextColor { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.ShowPicture")]
    public bool ShowPicture { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Picture")]
    [UIHint("Picture")]
    public int PictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.AclCustomerRoles")]
    public IList<int> SelectedCustomerRoleIds { get; set; }
    public IList<SelectListItem> AvailableCustomerRoles { get; set; }

    public IList<MegaMenuItemLocalizedModel> Locales { get; set; }

    public int ParentMenuItemId { get; set; }
    public int CategoryId { get; set; }
    public int ManufacturerId { get; set; }
    public int VendorId { get; set; }
    public int TopicId { get; set; }
    public int ProductTagId { get; set; }
    public int PageTypeId { get; set; }
    public int MenuItemTypeId { get; set; }
    public bool CanIncludeSubCategories { get; set; }
    public bool CanExcludeSubCategories { get; set; }
}

public class MegaMenuItemLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Url")]
    public string Url { get; set; }
}
