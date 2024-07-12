using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.ProductRequests
{
    public class ProductRequestPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageProductRequests = new PermissionRecord { Name = "NopStation Product Requests. Manage Product Requests", SystemName = "ManageNopStationProductRequests", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageProductRequests
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
                        ManageProductRequests
                    }
                )
            };
        }

    }
}
