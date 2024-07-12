using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Shipping.VendorAndState
{
    public class VendorAndStatePermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation Shipping By Vendor. Manage Configuration", SystemName = "ManageNopStationShippingByVendor", Category = "NopStation" };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageConfiguration
                    }
                )
            };
        }

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration
            };
        }
    }
}
