using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.Product360View
{
    public class Product360ViewPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageProduct360View = new PermissionRecord { Name = "NopStation Product 360 View. Manage Abandoned Carts", SystemName = "ManageNopStationProduct360View", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageProduct360View
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
                        ManageProduct360View
                    }
                )
            };
        }
    }
}
