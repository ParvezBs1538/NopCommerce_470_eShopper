using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Payments.EveryPay
{
    public class EveryPayPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation EveryPay. Manage every-pay", SystemName = "ManageNopStationEveryPay", Category = "NopStation" };

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
