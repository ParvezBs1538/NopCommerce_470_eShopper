using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Payout.Stripe
{
    public class StripePayoutPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageStripePayout = new PermissionRecord { Name = "NopStation stripe payout. Manage stripe payout", SystemName = "ManageNopStationStripePayout", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageStripePayout
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
                        ManageStripePayout
                    }
                )
            };
        }
    }
}
