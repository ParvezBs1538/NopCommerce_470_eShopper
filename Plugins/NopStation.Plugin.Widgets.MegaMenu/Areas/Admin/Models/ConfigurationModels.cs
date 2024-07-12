using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Models;

public partial record ConfigurationModel : BaseNopModel, ISettingsModel
{
    public ConfigurationModel()
    {
        SelectedCategoryIds = new List<int>();
        SelectedManufacturerIds = new List<int>();
        AvailableCategories = new List<SelectListItem>();
        AvailableManufacturers = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.Configuration.Fields.EnableMegaMenu")]
    public bool EnableMegaMenu { get; set; }
    public bool EnableMegaMenu_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.Configuration.Fields.MaxCategoryLevelsToShow")]
    public int MaxCategoryLevelsToShow { get; set; }
    public bool MaxCategoryLevelsToShow_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.Configuration.Fields.ShowNumberofCategoryProducts")]
    public bool ShowNumberofCategoryProducts { get; set; }
    public bool ShowNumberofCategoryProducts_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.Configuration.Fields.ShowNumberOfCategoryProductsIncludeSubcategories")]
    public bool ShowNumberOfCategoryProductsIncludeSubcategories { get; set; }
    public bool ShowNumberOfCategoryProductsIncludeSubcategories_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.Configuration.Fields.SelectedCategoryIds")]
    public IList<int> SelectedCategoryIds { get; set; }
    public bool SelectedCategoryIds_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.Configuration.Fields.ShowCategoryPicture")]
    public bool ShowCategoryPicture { get; set; }
    public bool ShowCategoryPicture_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.Configuration.Fields.DefaultCategoryIconId")]
    [UIHint("Picture")]
    public int DefaultCategoryIconId { get; set; }
    public bool DefaultCategoryIconId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.Configuration.Fields.ShowSubcategoryPicture")]
    public bool ShowSubcategoryPicture { get; set; }
    public bool ShowSubcategoryPicture_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.Configuration.Fields.HideManufacturers")]
    public bool HideManufacturers { get; set; }
    public bool HideManufacturers_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.Configuration.Fields.SelectedManufacturerIds")]
    public IList<int> SelectedManufacturerIds { get; set; }
    public bool SelectedManufacturerIds_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.Configuration.Fields.ShowManufacturerPicture")]
    public bool ShowManufacturerPicture { get; set; }
    public bool ShowManufacturerPicture_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.Configuration.Fields.ShowMainCategoryPictureRight")]
    public bool ShowMainCategoryPictureRight { get; set; }
    public bool ShowMainCategoryPictureRight_OverrideForStore { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; }
    public IList<SelectListItem> AvailableManufacturers { get; set; }
}