using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Payout.PayPal
{
    public class PayPalPayoutPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManagePayPalPayout = new PermissionRecord { Name = "NopStation paypal payout. Manage paypal payout", SystemName = "ManageNopStationPayPalPayout", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManagePayPalPayout
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
                        ManagePayPalPayout
                    }
                )
            };
        }
    }
}
