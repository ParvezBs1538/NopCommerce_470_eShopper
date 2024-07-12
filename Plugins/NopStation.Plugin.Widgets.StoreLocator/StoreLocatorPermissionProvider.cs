using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.StoreLocator
{
    public class StoreLocatorPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageStoreLocatorConfiguration = new PermissionRecord { Name = "NopStation store locator. Manage configuration", SystemName = "ManageNopStationStoreLocatorConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageStoreLocations = new PermissionRecord { Name = "NopStation store locator. Manage store locations", SystemName = "ManageNopStationStoreLocations", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageStoreLocatorConfiguration,
                ManageStoreLocations
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
                        ManageStoreLocatorConfiguration,
                        ManageStoreLocations
                    }
                )
            };
        }
    }
}
