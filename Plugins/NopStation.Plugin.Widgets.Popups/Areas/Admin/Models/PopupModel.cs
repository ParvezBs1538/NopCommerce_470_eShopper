using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.Popups.Areas.Admin.Models;

public record PopupModel : BaseNopEntityModel, ILocalizedModel<PopupLocalizedModel>, IStoreMappingSupportedModel,
    ICustomerConditionSupportedModel, IScheduleSupportedModel, IWidgetZoneSupportedModel, IAclSupportedModel, IProductConditionSupportedModel
{
    public PopupModel()
    {
        SelectedStoreIds = new List<int>();
        Locales = new List<PopupLocalizedModel>();

        SelectedCustomerRoleIds = new List<int>();
        AvailableCustomerRoles = new List<SelectListItem>();

        AvailableStores = new List<SelectListItem>();
        AvailableDeviceTypes = new List<SelectListItem>();
        AvailableColumnTypes = new List<SelectListItem>();
        AvailableContentTypes = new List<SelectListItem>();
        AvailablePositions = new List<SelectListItem>();

        AddWidgetZoneModel = new WidgetZoneModel();
        WidgetZoneSearchModel = new WidgetZoneSearchModel();

        Schedule = new ScheduleModel();
        CustomerConditionSearchModel = new CustomerConditionSearchModel();
        ProductConditionSearchModel = new ProductConditionSearchModel();
    }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.ColumnTypeId")]
    public int ColumnTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.ColumnTypeId")]
    public string ColumnTypeStr { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column1ContentTypeId")]
    public int Column1ContentTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column1Text")]
    public string Column1Text { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.DeviceTypeId")]
    public int DeviceTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.DeviceTypeId")]
    public string DeviceTypeStr { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column1DesktopPictureId")]
    [UIHint("Picture")]
    public int Column1DesktopPictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column1MobilePictureId")]
    [UIHint("Picture")]
    public int Column1MobilePictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column1PopupUrl")]
    public string Column1PopupUrl { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column2ContentTypeId")]
    public int Column2ContentTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column2Text")]
    public string Column2Text { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column2DesktopPictureId")]
    [UIHint("Picture")]
    public int Column2DesktopPictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column2MobilePictureId")]
    [UIHint("Picture")]
    public int Column2MobilePictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column2PopupUrl")]
    public string Column2PopupUrl { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.EnableStickyButton")]
    public bool EnableStickyButton { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.StickyButtonColor")]
    public string StickyButtonColor { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.StickyButtonPositionId")]
    public int StickyButtonPositionId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.StickyButtonText")]
    public string StickyButtonText { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.OpenPopupOnLoadPage")]
    public bool OpenPopupOnLoadPage { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.DelayTime")]
    public int DelayTime { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.AllowCustomerToSelectDoNotShowThisPopupAgain")]
    public bool AllowCustomerToSelectDoNotShowThisPopupAgain { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.PreSelectedDoNotShowThisPopupAgain")]
    public bool PreSelectedDoNotShowThisPopupAgain { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.CssClass")]
    public string CssClass { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Active")]
    public bool Active { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.CreatedOn")]
    public DateTime CreatedOn { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.LimitedToStores")]
    public IList<int> SelectedStoreIds { get; set; }
    public IList<SelectListItem> AvailableStores { get; set; }

    public IList<SelectListItem> AvailableDeviceTypes { get; set; }
    public IList<SelectListItem> AvailableColumnTypes { get; set; }
    public IList<SelectListItem> AvailableContentTypes { get; set; }
    public IList<SelectListItem> AvailablePositions { get; set; }

    public IList<PopupLocalizedModel> Locales { get; set; }

    public ScheduleModel Schedule { get; set; }

    public WidgetZoneModel AddWidgetZoneModel { get; set; }
    public WidgetZoneSearchModel WidgetZoneSearchModel { get; set; }

    public CustomerConditionSearchModel CustomerConditionSearchModel { get; set; }

    public ProductConditionSearchModel ProductConditionSearchModel { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.AclCustomerRoles")]
    public IList<int> SelectedCustomerRoleIds { get; set; }
    public IList<SelectListItem> AvailableCustomerRoles { get; set; }
}

public class PopupLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column1Text")]
    public string Column1Text { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column1DesktopPictureId")]
    [UIHint("Picture")]
    public int Column1DesktopPictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column1MobilePictureId")]
    [UIHint("Picture")]
    public int Column1MobilePictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column1PopupUrl")]
    public string Column1PopupUrl { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column2Text")]
    public string Column2Text { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column2DesktopPictureId")]
    [UIHint("Picture")]
    public int Column2DesktopPictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column2MobilePictureId")]
    [UIHint("Picture")]
    public int Column2MobilePictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.Column2PopupUrl")]
    public string Column2PopupUrl { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.StickyButtonPositionId")]
    public int StickyButtonPositionId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.Fields.StickyButtonText")]
    public string StickyButtonText { get; set; }
}
