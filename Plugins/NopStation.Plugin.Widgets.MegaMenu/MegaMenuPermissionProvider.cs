using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.MegaMenu;

public class MegaMenuPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageMegaMenu = new PermissionRecord { Name = "NopStation mega menu. Manage mega-menu", SystemName = "ManageNopStationMegaMenu", Category = "NopStation" };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageMegaMenu
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
                    ManageMegaMenu
                }
            )
        };
    }

}
