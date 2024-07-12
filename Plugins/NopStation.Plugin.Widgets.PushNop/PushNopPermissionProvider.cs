using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.PushNop
{
    public class PushNopPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageSmartGroupNotifications = new PermissionRecord { Name = "NopStation PushNop. Manage Smart Group Notifications", SystemName = "NopStationPushNopManageSmartGroupNotifications", Category = "NopStation" };
        public static readonly PermissionRecord ManageReports = new PermissionRecord { Name = "NopStation PushNop. Manage Reports", SystemName = "NopStationPushNopManageReports", Category = "NopStation" };
        public static readonly PermissionRecord ManageSmartGroups = new PermissionRecord { Name = "NopStation PushNop. Manage Smart Groups", SystemName = "NopStationPushNopManageSmartGroups", Category = "NopStation" };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageSmartGroupNotifications,
                ManageReports,
                ManageSmartGroups
            };
        }

        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageSmartGroupNotifications,
                        ManageReports,
                        ManageSmartGroups
                    }
                )
            };
        }
    }
}
