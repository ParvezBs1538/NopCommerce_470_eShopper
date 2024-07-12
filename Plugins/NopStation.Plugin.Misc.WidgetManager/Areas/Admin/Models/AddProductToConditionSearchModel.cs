using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

public partial record AddProductToConditionSearchModel : BaseSearchModel
{
    #region Ctor

    public AddProductToConditionSearchModel()
    {
        AvailableCategories = new List<SelectListItem>();
        AvailableManufacturers = new List<SelectListItem>();
        AvailableStores = new List<SelectListItem>();
        AvailableWarehouses = new List<SelectListItem>();
        AvailableVendors = new List<SelectListItem>();
        AvailableProductTypes = new List<SelectListItem>();
        AvailablePublishedOptions = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    public int EntityId { get; set; }

    public string EntityName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Products.List.SearchProductName")]
    public string SearchProductName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Products.List.SearchCategory")]
    public int SearchCategoryId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Products.List.SearchIncludeSubCategories")]
    public bool SearchIncludeSubCategories { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Products.List.SearchManufacturer")]
    public int SearchManufacturerId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Products.List.SearchStore")]
    public int SearchStoreId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Products.List.SearchVendor")]
    public int SearchVendorId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Products.List.SearchWarehouse")]
    public int SearchWarehouseId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Products.List.SearchProductType")]
    public int SearchProductTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Products.List.SearchPublished")]
    public int SearchPublishedId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Conditions.Products.List.GoDirectlyToSku")]
    public string GoDirectlyToSku { get; set; }

    public bool IsLoggedInAsVendor { get; set; }

    public bool AllowVendorsToImportProducts { get; set; }

    public bool HideStoresList { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; }

    public IList<SelectListItem> AvailableManufacturers { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<SelectListItem> AvailableWarehouses { get; set; }

    public IList<SelectListItem> AvailableVendors { get; set; }

    public IList<SelectListItem> AvailableProductTypes { get; set; }

    public IList<SelectListItem> AvailablePublishedOptions { get; set; }

    #endregion
}