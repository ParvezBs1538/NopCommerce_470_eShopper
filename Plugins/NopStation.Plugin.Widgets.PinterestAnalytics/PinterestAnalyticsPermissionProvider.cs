using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.PinterestAnalytics
{
    public class PinterestAnalyticsPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManagePinterestAnalytics = new PermissionRecord { Name = "NopStation Pinterest Analytics. Manage Pinterest Analytics", SystemName = "ManageNopStationPinterestAnalytics", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManagePinterestAnalytics
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
                        ManagePinterestAnalytics
                    }
                )
            };
        }
    }
}
