namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Infrastructure.Cache
{
    public static class PWAEntityCacheDefaults
    {
        public static string CheckSubscription => "Nopstation.pwa.checkpushmanagersubscription";

        public static string WebAppDevicesAllCacheKey => "Admin.NopStation.PWA.WebAppDevices.all";

        public static string WebAppDevicesCustomerCacheKey => "Admin.NopStation.PWA.WebAppDevices.customer-{0}-{1}-{2}-{3}-{4}-{5}";

        public static string WebAppDevicesPrefixCacheKey => "Admin.NopStation.PWA.WebAppDevices.";
    }
}
