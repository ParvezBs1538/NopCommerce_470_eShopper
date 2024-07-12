using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Payments.CreditWallet
{
    public class CreditWalletPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageCustomerCreditPayment = new PermissionRecord { Name = "NopStation Customer Credit Payment. Manage Customer Credit Payment", SystemName = "ManageNopStationCustomerCreditPayment", Category = "NopStation" };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageCustomerCreditPayment
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageCustomerCreditPayment
            };
        }
    }
}
