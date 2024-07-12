using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp
{
    public class ProgressiveWebAppPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation Progressive Web App. Configuration", SystemName = "ManageProgressiveWebAppConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageTemplates = new PermissionRecord { Name = "NopStation Progressive Web App. Manage Templates", SystemName = "ManageProgressiveWebAppTemplates", Category = "NopStation" };
        public static readonly PermissionRecord ManageAnnouncements = new PermissionRecord { Name = "NopStation Progressive Web App. Manage Announcements", SystemName = "ManageProgressiveWebAppAnnouncements", Category = "NopStation" };
        public static readonly PermissionRecord ManageQueuedNotifications = new PermissionRecord { Name = "NopStation Progressive Web App. Manage Queued Notifications", SystemName = "ManageProgressiveWebAppQueuedNotifications", Category = "NopStation" };
        public static readonly PermissionRecord ManageDevices = new PermissionRecord { Name = "NopStation Progressive Web App. Manage Devices", SystemName = "ManageProgressiveWebAppDevices", Category = "NopStation" };

        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageConfiguration,
                        ManageTemplates,
                        ManageAnnouncements,
                        ManageQueuedNotifications,
                        ManageDevices
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageTemplates,
                ManageAnnouncements,
                ManageQueuedNotifications,
                ManageDevices
            };
        }
    }
}
