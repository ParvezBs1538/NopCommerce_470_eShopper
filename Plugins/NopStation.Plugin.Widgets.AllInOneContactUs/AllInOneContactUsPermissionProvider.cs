using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.Announcement
{
    public class AllInOneContactUsPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageAllInOneContactUs = new PermissionRecord { Name = "NopStation AllInOneContactUs. Manage all in one contact us", SystemName = "ManageNopStationAllInOneContactUs", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageAllInOneContactUs
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
                        ManageAllInOneContactUs,
                    }
                )
            };
        }
    }
}
