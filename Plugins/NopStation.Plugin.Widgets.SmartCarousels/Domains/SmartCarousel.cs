using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;

namespace NopStation.Plugin.Widgets.SmartCarousels.Domains;

public partial class SmartCarousel : BaseEntity, ILocalizedEntity, IStoreMappingSupported, IAclSupported,
    IWidgetZoneSupported, IScheduleSupported, IConditionSupported, IProductConditionSupported,
    ICustomerConditionSupported, ISoftDeletedEntity
{
    #region Default properties

    public string Name { get; set; }

    public string Title { get; set; }

    public bool DisplayTitle { get; set; }

    public bool Active { get; set; }

    public int CarouselTypeId { get; set; }

    public int ProductSourceTypeId { get; set; }

    public bool ShowBackground { get; set; }

    public int BackgroundTypeId { get; set; }

    public string BackgroundColor { get; set; }

    public int BackgroundPictureId { get; set; }

    public string CustomUrl { get; set; }

    public int MaxProductsToShow { get; set; }

    public int DisplayOrder { get; set; }

    public string CustomCssClass { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    public bool LimitedToStores { get; set; }

    public bool SubjectToAcl { get; set; }

    public bool Deleted { get; set; }

    public bool HasWidgetZoneMappingApplied { get; set; }

    #endregion

    #region Slider properties

    public bool EnableAutoPlay { get; set; }

    public int AutoPlayTimeout { get; set; }

    public bool AutoPlayHoverPause { get; set; }

    public bool EnableLoop { get; set; }

    public int StartPosition { get; set; }

    public bool Center { get; set; }

    public bool EnableKeyboardControl { get; set; }

    public bool KeyboardControlOnlyInViewport { get; set; }

    public bool EnableNavigation { get; set; }

    public bool EnableLazyLoad { get; set; }

    public bool EnablePagination { get; set; }

    public bool PaginationClickable { get; set; }

    public int PaginationTypeId { get; set; }

    public bool PaginationDynamicBullets { get; set; }

    public int PaginationDynamicMainBullets { get; set; }

    public PaginationType PaginationType
    {
        get => (PaginationType)PaginationTypeId;
        set => PaginationTypeId = (int)value;
    }

    #endregion

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

    public CarouselType CarouselType
    {
        get => (CarouselType)CarouselTypeId;
        set => CarouselTypeId = (int)value;
    }

    public ProductSourceType ProductSourceType
    {
        get => (ProductSourceType)ProductSourceTypeId;
        set => ProductSourceTypeId = (int)value;
    }

    public BackgroundType BackgroundType
    {
        get => (BackgroundType)BackgroundTypeId;
        set => BackgroundTypeId = (int)value;
    }
}
