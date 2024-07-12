using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.MegaMenu;

public class MegaMenuSettings : ISettings
{
    public bool EnableMegaMenu { get; set; }

    public int MaxCategoryLevelsToShow { get; set; }

    public bool ShowNumberOfCategoryProducts { get; set; }

    public bool ShowNumberOfCategoryProductsIncludeSubcategories { get; set; }

    public bool ShowCategoryPicture { get; set; }

    public bool ShowSubcategoryPicture { get; set; }

    public bool HideManufacturers { get; set; }

    public string SelectedManufacturerIds { get; set; }

    public string SelectedCategoryIds { get; set; }

    public bool ShowManufacturerPicture { get; set; }

    public int DefaultCategoryIconId { get; set; }
    public bool ShowMainCategoryPictureRight { get; set; }
}
