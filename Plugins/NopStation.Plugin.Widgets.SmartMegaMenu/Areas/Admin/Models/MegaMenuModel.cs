using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;

public record MegaMenuModel : BaseNopEntityModel, ILocalizedModel<MegaMenuLocalizedModel>, IStoreMappingSupportedModel,
    IWidgetZoneSupportedModel
{
    public MegaMenuModel()
    {
        SelectedStoreIds = new List<int>();
        AvailableStores = new List<SelectListItem>();
        Locales = new List<MegaMenuLocalizedModel>();

        AvailablePageTypes = new List<SelectListItem>();
        AvailableViewTypes = new List<SelectListItem>();

        AddWidgetZoneModel = new WidgetZoneModel();
        WidgetZoneSearchModel = new WidgetZoneSearchModel();

        AddCustomLinkItemModel = new MegaMenuItemModel();
        AddCategoryToMegaMenuSearchModel = new AddCategoryToMegaMenuSearchModel();
        AddManufacturerToMegaMenuSearchModel = new AddManufacturerToMegaMenuSearchModel();
        AddVendorToMegaMenuSearchModel = new AddVendorToMegaMenuSearchModel();
        AddTopicToMegaMenuSearchModel = new AddTopicToMegaMenuSearchModel();
        AddProductTagToMegaMenuSearchModel = new AddProductTagToMegaMenuSearchModel();
    }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.Active")]
    public bool Active { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.ViewType")]
    public int ViewTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.WithoutImages")]
    public bool WithoutImages { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.CssClass")]
    public string CssClass { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.CreatedOn")]
    public DateTime CreatedOn { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.UpdatedOn")]
    public DateTime UpdatedOn { get; set; }

    public IList<SelectListItem> AvailablePageTypes { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.LimitedToStores")]
    public IList<int> SelectedStoreIds { get; set; }
    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<MegaMenuLocalizedModel> Locales { get; set; }

    public IList<SelectListItem> AvailableViewTypes { get; set; }

    public WidgetZoneModel AddWidgetZoneModel { get; set; }
    public WidgetZoneSearchModel WidgetZoneSearchModel { get; set; }

    public MegaMenuItemModel AddCustomLinkItemModel { get; set; }
    public AddCategoryToMegaMenuSearchModel AddCategoryToMegaMenuSearchModel { get; set; }
    public AddManufacturerToMegaMenuSearchModel AddManufacturerToMegaMenuSearchModel { get; set; }
    public AddVendorToMegaMenuSearchModel AddVendorToMegaMenuSearchModel { get; set; }
    public AddTopicToMegaMenuSearchModel AddTopicToMegaMenuSearchModel { get; set; }
    public AddProductTagToMegaMenuSearchModel AddProductTagToMegaMenuSearchModel { get; set; }
}

public class MegaMenuLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.Name")]
    public string Name { get; set; }
}
