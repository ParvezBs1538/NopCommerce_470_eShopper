using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Models;

public record SmartSliderModel : BaseNopEntityModel, IStoreMappingSupportedModel,
    IAclSupportedModel, IWidgetZoneSupportedModel, IScheduleSupportedModel,
    ICustomerConditionSupportedModel, IProductConditionSupportedModel
{
    public SmartSliderModel()
    {
        AvaliableBackgroundTypes = new List<SelectListItem>();
        AvailablePaginationTypes = new List<SelectListItem>();
        AvailableEffectTypes = new List<SelectListItem>();
        SmartSliderItemSearchModel = new SmartSliderItemSearchModel();

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

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.Active")]
    public bool Active { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.ShowBackground")]
    public bool ShowBackground { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.BackgroundType")]
    public int BackgroundTypeId { get; set; }
    public string BackgroundTypeStr { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.BackgroundColor")]
    public string BackgroundColor { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.BackgroundPicture")]
    [UIHint("Picture")]
    public int BackgroundPictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.CustomCssClass")]
    public string CustomCssClass { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.CreatedOn")]
    public DateTime CreatedOn { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.UpdatedOn")]
    public DateTime UpdatedOn { get; set; }


    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.EnableAutoPlay")]
    public bool EnableAutoPlay { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.AutoPlayTimeout")]
    public int AutoPlayTimeout { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.AutoPlayHoverPause")]
    public bool AutoPlayHoverPause { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.EnableLoop")]
    public bool EnableLoop { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.EnableKeyboardControl")]
    public bool EnableKeyboardControl { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.KeyboardControlOnlyInViewport")]
    public bool KeyboardControlOnlyInViewport { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.EnableNavigation")]
    public bool EnableNavigation { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.EnableLazyLoad")]
    public bool EnableLazyLoad { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.EnablePagination")]
    public bool EnablePagination { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.PaginationClickable")]
    public bool PaginationClickable { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.PaginationType")]
    public int PaginationTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.PaginationDynamicBullets")]
    public bool PaginationDynamicBullets { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.PaginationDynamicMainBullets")]
    public int PaginationDynamicMainBullets { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.StartPosition")]
    public int StartPosition { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.EnableZoom")]
    public bool EnableZoom { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.ZoomMaximumRatio")]
    public decimal ZoomMaximumRatio { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.ZoomMinimumRatio")]
    public decimal ZoomMinimumRatio { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.ToggleZoom")]
    public bool ToggleZoom { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.EnableEffect")]
    public bool EnableEffect { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.EffectType")]
    public int EffectTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.EnableMousewheelControl")]
    public bool EnableMousewheelControl { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.MousewheelControlForceToAxis")]
    public bool MousewheelControlForceToAxis { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.VerticalDirection")]
    public bool VerticalDirection { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.AutoHeight")]
    public bool AutoHeight { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.AllowTouchMove")]
    public bool AllowTouchMove { get; set; }

    public IList<SelectListItem> AvaliableBackgroundTypes { get; set; }
    public IList<SelectListItem> AvailablePaginationTypes { get; set; }
    public IList<SelectListItem> AvailableEffectTypes { get; set; }

    public SmartSliderItemSearchModel SmartSliderItemSearchModel { get; set; }

    #region Inherited

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.LimitedToStores")]
    public IList<int> SelectedStoreIds { get; set; }
    public IList<SelectListItem> AvailableStores { get; set; }

    public ScheduleModel Schedule { get; set; }

    public WidgetZoneModel AddWidgetZoneModel { get; set; }
    public WidgetZoneSearchModel WidgetZoneSearchModel { get; set; }

    public CustomerConditionSearchModel CustomerConditionSearchModel { get; set; }

    public ProductConditionSearchModel ProductConditionSearchModel { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Fields.AclCustomerRoles")]
    public IList<int> SelectedCustomerRoleIds { get; set; }
    public IList<SelectListItem> AvailableCustomerRoles { get; set; }

    #endregion
}
