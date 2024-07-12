using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.PrevNextProduct;

public class PrevNextProductPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation prev/next product. Manage configuration", SystemName = "ManageNopStationPrevNextProduct", Category = "NopStation" };

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