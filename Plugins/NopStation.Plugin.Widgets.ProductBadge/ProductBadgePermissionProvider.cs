using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.ProductBadge;

public class ProductBadgePermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageProductBadge = new PermissionRecord { Name = "NopStation product badge. Manage product badge", SystemName = "ManageNopStationProductBadge", Category = "NopStation" };

    public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
    {
        return new HashSet<(string, PermissionRecord[])>
        {
            (
                NopCustomerDefaults.AdministratorsRoleName,
                new[]
                {
                    ManageProductBadge
                }
            )
        };
    }

    public IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageProductBadge
        };
    }
}