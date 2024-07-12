using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public record SmartCarouselModel : BaseNopEntityModel, ILocalizedModel<CarouselLocalizedModel>,
    IStoreMappingSupportedModel, IAclSupportedModel, IWidgetZoneSupportedModel, IScheduleSupportedModel,
    ICustomerConditionSupportedModel, IProductConditionSupportedModel
{
    public SmartCarouselModel()
    {
        AvaliableBackgroundTypes = new List<SelectListItem>();
        AvailableProductSources = new List<SelectListItem>();
        AvailableCarouselTypes = new List<SelectListItem>();
        AvailablePaginationTypes = new List<SelectListItem>();

        CarouselProductSearchModel = new SmartCarouselProductSearchModel();
        CarouselCategorySearchModel = new SmartCarouselCategorySearchModel();
        CarouselManufacturerSearchModel = new SmartCarouselManufacturerSearchModel();
        CarouselVendorSearchModel = new SmartCarouselVendorSearchModel();
        CarouselPictureSearchModel = new SmartCarouselPictureSearchModel();
        Locales = new List<CarouselLocalizedModel>();

        SelectedStoreIds = new List<int>();
        AvailableStores = new List<SelectListItem>();

        SelectedCustomerRoleIds = new List<int>();
        AvailableCustomerRoles = new List<SelectListItem>();

        AddWidgetZoneModel = new WidgetZoneModel();
        WidgetZoneSearchModel = new WidgetZoneSearchModel();

        Schedule = new ScheduleModel();
        CustomerConditionSearchModel = new CustomerConditionSearchModel();
        ProductConditionSearchModel = new ProductConditionSearchModel();
    }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.DisplayTitle")]
    public bool DisplayTitle { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.Active")]
    public bool Active { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.CarouselType")]
    public int CarouselTypeId { get; set; }
    public string CarouselTypeStr { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.ProductSourceType")]
    public int ProductSourceTypeId { get; set; }
    public string ProductSourceTypeStr { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.MaxProductsToShow")]
    public int MaxProductsToShow { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.ShowBackground")]
    public bool ShowBackground { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.BackgroundType")]
    public int BackgroundTypeId { get; set; }
    public string BackgroundTypeStr { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.BackgroundColor")]
    public string BackgroundColor { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.BackgroundPicture")]
    [UIHint("Picture")]
    public int BackgroundPictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.CustomUrl")]
    public string CustomUrl { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.CustomCssClass")]
    public string CustomCssClass { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }


    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.EnableAutoPlay")]
    public bool EnableAutoPlay { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.AutoPlayTimeout")]
    public int AutoPlayTimeout { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.AutoPlayHoverPause")]
    public bool AutoPlayHoverPause { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.EnableLoop")]
    public bool EnableLoop { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.StartPosition")]
    public int StartPosition { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.Center")]
    public bool Center { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.EnableKeyboardControl")]
    public bool EnableKeyboardControl { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.KeyboardControlOnlyInViewport")]
    public bool KeyboardControlOnlyInViewport { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.EnableNavigation")]
    public bool EnableNavigation { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.EnableLazyLoad")]
    public bool EnableLazyLoad { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.EnablePagination")]
    public bool EnablePagination { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.PaginationClickable")]
    public bool PaginationClickable { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.PaginationType")]
    public int PaginationTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.PaginationDynamicBullets")]
    public bool PaginationDynamicBullets { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.PaginationDynamicMainBullets")]
    public int PaginationDynamicMainBullets { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.CreatedOn")]
    public DateTime CreatedOn { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.UpdatedOn")]
    public DateTime UpdatedOn { get; set; }

    public SmartCarouselProductSearchModel CarouselProductSearchModel { get; set; }
    public SmartCarouselCategorySearchModel CarouselCategorySearchModel { get; set; }
    public SmartCarouselManufacturerSearchModel CarouselManufacturerSearchModel { get; set; }
    public SmartCarouselVendorSearchModel CarouselVendorSearchModel { get; set; }
    public SmartCarouselPictureSearchModel CarouselPictureSearchModel { get; set; }

    public IList<SelectListItem> AvailableProductSources { get; set; }
    public IList<SelectListItem> AvailableCarouselTypes { get; set; }
    public IList<SelectListItem> AvailablePaginationTypes { get; set; }

    public IList<CarouselLocalizedModel> Locales { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.LimitedToStores")]
    public IList<int> SelectedStoreIds { get; set; }
    public IList<SelectListItem> AvailableStores { get; set; }

    public ScheduleModel Schedule { get; set; }

    public WidgetZoneModel AddWidgetZoneModel { get; set; }
    public WidgetZoneSearchModel WidgetZoneSearchModel { get; set; }

    public CustomerConditionSearchModel CustomerConditionSearchModel { get; set; }

    public ProductConditionSearchModel ProductConditionSearchModel { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.AclCustomerRoles")]
    public IList<int> SelectedCustomerRoleIds { get; set; }
    public IList<SelectListItem> AvailableCustomerRoles { get; set; }

    public IList<SelectListItem> AvaliableBackgroundTypes { get; set; }
}

public class CarouselLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Fields.CustomUrl")]
    public string CustomUrl { get; set; }
}