using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;

namespace NopStation.Plugin.Widgets.Popups.Domains;

public class Popup : BaseEntity, ILocalizedEntity, IStoreMappingSupported, IAclSupported, ISoftDeletedEntity,
    IScheduleSupported, IConditionSupported, ICustomerConditionSupported, IProductConditionSupported
{
    public string Name { get; set; }

    public int DeviceTypeId { get; set; }

    public int ColumnTypeId { get; set; }

    public int Column1ContentTypeId { get; set; }

    public string Column1Text { get; set; }

    public int Column1DesktopPictureId { get; set; }

    public int Column1MobilePictureId { get; set; }

    public string Column1PopupUrl { get; set; }

    public int Column2ContentTypeId { get; set; }

    public string Column2Text { get; set; }

    public int Column2DesktopPictureId { get; set; }

    public int Column2MobilePictureId { get; set; }

    public string Column2PopupUrl { get; set; }

    public bool EnableStickyButton { get; set; }

    public int StickyButtonPositionId { get; set; }

    public string StickyButtonColor { get; set; }

    public string StickyButtonText { get; set; }

    public bool OpenPopupOnLoadPage { get; set; }

    public int DelayTime { get; set; }

    public bool AllowCustomerToSelectDoNotShowThisPopupAgain { get; set; }

    public bool PreSelectedDoNotShowThisPopupAgain { get; set; }

    public string CssClass { get; set; }

    public bool Active { get; set; }

    public bool LimitedToStores { get; set; }

    public bool Deleted { get; set; }

    public bool SubjectToAcl { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    #region Schedules

    public DateTime? AvaliableDateTimeFromUtc { get; set; }

    public DateTime? AvaliableDateTimeToUtc { get; set; }

    public int ScheduleTypeId { get; set; }

    public long? TimeOfDayFromTicksUtc { get; set; }

    public long? TimeOfDayToTicksUtc { get; set; }

    public ScheduleType ScheduleType
    {
        get => (ScheduleType)ScheduleTypeId;
        set => ScheduleTypeId = (int)value;
    }

    #endregion

    #region Conditions

    public bool HasConditionApplied { get; set; }

    public bool EnableCondition { get; set; }

    public bool HasCustomerConditionApplied { get; set; }

    public bool HasProductConditionApplied { get; set; }

    #endregion

    public Position StickyButtonPosition
    {
        get => (Position)StickyButtonPositionId;
        set => StickyButtonPositionId = (int)value;
    }

    public ColumnType ColumnType
    {
        get => (ColumnType)ColumnTypeId;
        set => ColumnTypeId = (int)value;
    }

    public ContentType Column1ContentType
    {
        get => (ContentType)Column1ContentTypeId;
        set => Column1ContentTypeId = (int)value;
    }

    public ContentType Column2ContentType
    {
        get => (ContentType)Column2ContentTypeId;
        set => Column2ContentTypeId = (int)value;
    }

    public DeviceType DeviceType
    {
        get => (DeviceType)DeviceTypeId;
        set => DeviceTypeId = (int)value;
    }
}
