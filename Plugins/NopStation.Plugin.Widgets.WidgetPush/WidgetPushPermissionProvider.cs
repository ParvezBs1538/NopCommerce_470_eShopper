using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.WidgetPush
{
    public class WidgetPushPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageWidgetPush = new PermissionRecord { Name = "NopStation widget push. Manage widget push", SystemName = "ManageNopStationWidgetPush", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageWidgetPush
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
                        ManageWidgetPush,
                    }
                )
            };
        }
    }
}
