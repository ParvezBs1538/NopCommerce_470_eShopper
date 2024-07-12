using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.AdminApi;

public class AdminApiPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageConfiguration = new() {
        Name = "NopStation Admin api. Configuration",
        SystemName = "ManageAdminApiConfiguration", Category = "NopStation"
    };


    public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
    {
        return new ()
        {
            (
                NopCustomerDefaults.AdministratorsRoleName,
                [ManageConfiguration]
            )
        };
    }

    public IEnumerable<PermissionRecord> GetPermissions()
    {
        return [ManageConfiguration];
    }
}
