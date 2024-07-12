using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.QuickView;

public class QuickViewPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageQuickView = new PermissionRecord { Name = "NopStation quick view. Manage quick view", SystemName = "ManageNopStationQuickView", Category = "NopStation" };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageQuickView
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
                    ManageQuickView
                }
            )
        };
    }
}
