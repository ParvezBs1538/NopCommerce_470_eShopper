using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.ProductTabs
{
    public class ProductTabPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageProductTab = new PermissionRecord { Name = "Admin area. Manage NopStation product tab", SystemName = "ManageNopStationProductTab", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageProductTab
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
                        ManageProductTab
                    }
                )
            };
        }
    }
}
