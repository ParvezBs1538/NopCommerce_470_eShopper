using Nop.Core.Caching;

namespace NopStation.Plugin.Misc.WidgetManager;

public class WidgetManagerDefaults
{
    #region Widget zone mapping caching defaults

    public static CacheKey EntityWidgetZonesCacheKey => new("Nopstation.widgetmanager.widgetzones.byentity-{0}-{1}");

    public static CacheKey DomainWidgetZonesCacheKey => new("Nopstation.widgetmanager.widgetzones.bydomain-{0}");

    public static CacheKey EntityWidgetZoneMappingsCacheKey => new("Nopstation.widgetmanager.widgetzonemappings.byentity-{0}-{1}");

    public static CacheKey WidgetZoneMappingsCacheKey => new("Nopstation.widgetmanager.widgetzonemapping.mappings.{0}");

    public static CacheKey WidgetZoneMappingExistsCacheKey => new("Nopstation.widgetmanager.widgetzonemapping.exists.{0}-{1}");

    public static CacheKey EntityWidgetZoneMappingCacheKey => new("Nopstation.widgetmanager.widgetzonemapping.byentity-{0}-{1}-{2}", EntityWidgetZoneMappingPrefiix);

    public static string EntityWidgetZoneMappingPrefiix => new("Nopstation.widgetmanager.widgetzonemapping.byentity-{0}-{1}");

    #endregion

    #region Schedule monthly mapping caching defaults

    public static CacheKey ScheduleMonthlyDaysCacheKey => new("Nopstation.widgetmanager.schedulemonthlymapping.days{0}-{1}");

    public static CacheKey ScheduleMonthlyMappingsCacheKey => new("Nopstation.widgetmanager.schedulemonthlymapping.mappings.{0}-{1}");

    #endregion

    #region Schedule weekly mapping caching defaults

    public static CacheKey ScheduleWeeklyDaysCacheKey => new("Nopstation.widgetmanager.scheduleweeklymapping.days{0}-{1}");

    public static CacheKey ScheduleWeeklyMappingsCacheKey => new("Nopstation.widgetmanager.scheduleweeklymapping.mappings.{0}-{1}");

    #endregion

    #region Customer condition mapping caching defaults

    public static CacheKey EntityCustomerConditionMappingCacheKey => new("Nopstation.widgetmanager.customerconditionmapping.byentity-{0}-{1}-{2}", EntityCustomerConditionMappingPrefiix);

    public static string EntityCustomerConditionMappingPrefiix => new("Nopstation.widgetmanager.customerconditionmapping.byentity-{0}-{1}");

    public static CacheKey EntityCustomerConditionsCacheKey => new("Nopstation.widgetmanager.customerconditions.byentity-{0}-{1}");

    public static CacheKey EntityCustomerConditionMappingsCacheKey => new("Nopstation.widgetmanager.customerconditionmappings.byentity-{0}-{1}");

    public static CacheKey CustomerConditionMappingExistsCacheKey => new("Nopstation.widgetmanager.customerconditionmapping.exists.{0}-{1}");

    #endregion

    #region Product condition mapping caching defaults

    public static CacheKey EntityProductConditionMappingCacheKey => new("Nopstation.widgetmanager.productconditionmapping.byentity-{0}-{1}-{2}", EntityProductConditionMappingPrefiix);

    public static string EntityProductConditionMappingPrefiix => new("Nopstation.widgetmanager.productconditionmapping.byentity-{0}-{1}");

    public static CacheKey EntityProductConditionsCacheKey => new("Nopstation.widgetmanager.productconditions.byentity-{0}-{1}");

    public static CacheKey EntityProductConditionMappingsCacheKey => new("Nopstation.widgetmanager.productconditionmappings.byentity-{0}-{1}");

    public static CacheKey ProductConditionMappingExistsCacheKey => new("Nopstation.widgetmanager.productconditionmapping.exists.{0}-{1}");

    #endregion
}
