using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance
{
    public class PickupInStorePermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManagePickupInStore = new PermissionRecord { Name = "NopStation pickup point. Manage pickup point advance", SystemName = "ManageNopStationPickupInStoreAdvance", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManagePickupInStore
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
                        ManagePickupInStore
                    }
                )
            };
        }
    }
}
