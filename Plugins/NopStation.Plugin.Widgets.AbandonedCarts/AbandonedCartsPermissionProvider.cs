using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.AbandonedCarts
{
    public class AbandonedCartsPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageAbandonedCarts = new PermissionRecord { Name = "NopStation Abandoned Carts. Manage Abandoned Carts", SystemName = "ManageNopStationAbandonedCarts", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageAbandonedCarts
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
                        ManageAbandonedCarts
                    }
                )
            };
        }
    }
}
