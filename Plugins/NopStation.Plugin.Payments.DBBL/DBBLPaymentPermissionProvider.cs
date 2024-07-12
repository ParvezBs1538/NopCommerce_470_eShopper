using System.Collections.Generic;
using Nop.Services.Security;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Customers;

namespace NopStation.Plugin.Payments.DBBL
{
    public class DBBLPaymentPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation DBBL. Manage DBBL", SystemName = "ManageNopStationDBBLPayment", Category = "NopStation" };

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

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration
            };
        }
    }
}