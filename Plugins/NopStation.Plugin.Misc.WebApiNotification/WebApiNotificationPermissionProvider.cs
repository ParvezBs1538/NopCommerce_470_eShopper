using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.WebApiNotification
{
    public class WebApiNotificationPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageCampaigns = new PermissionRecord { Name = "NopStation app push notification. Manage Campaigns", SystemName = "NopStationWebApiNotificationManageCampaigns", Category = "NopStation" };
        public static readonly PermissionRecord ManageReports = new PermissionRecord { Name = "NopStation app push notification. Manage Reports", SystemName = "NopStationWebApiNotificationManageReports", Category = "NopStation" };
        public static readonly PermissionRecord ManageTemplates = new PermissionRecord { Name = "NopStation app push notification. Manage Templates", SystemName = "NopStationWebApiNotificationManageTemplates", Category = "NopStation" };
        public static readonly PermissionRecord ManageQueuedNotifications = new PermissionRecord { Name = "NopStation app push notification. Queued Notifications", SystemName = "NopStationWebApiNotificationManageQueuedNotification", Category = "NopStation" };
        public static readonly PermissionRecord ManageSmartGroups = new PermissionRecord { Name = "NopStation app push notification. Manage Smart Groups", SystemName = "NopStationWebApiNotificationManageSmartGroups", Category = "NopStation" };
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation app push notification. Manage Configuration", SystemName = "NopStationWebApiNotificationManageConfiguration", Category = "NopStation" };

        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageCampaigns,
                        ManageReports,
                        ManageTemplates,
                        ManageQueuedNotifications,
                        ManageSmartGroups,
                        ManageConfiguration
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageCampaigns,
                ManageReports,
                ManageTemplates,
                ManageQueuedNotifications,
                ManageSmartGroups,
                ManageConfiguration
            };
        }
    }
}
