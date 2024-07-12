using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.CancelOrder
{
    public class CancelOrderPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageCancelOrder = new PermissionRecord { Name = "NopStation Cancel Order. Manage Cancel Order", SystemName = "ManageNopStationCancelOrder", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageCancelOrder
            };
        }

        HashSet<(string systemRoleName, PermissionRecord[] permissions)> IPermissionProvider.GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageCancelOrder
                    }
                )
            };
        }

    }
}
