using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.SmartMegaMenu;

public class SmartMegaMenuPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageMegaMenu = new PermissionRecord { Name = "NopStation Smart Mega Menu. Manage mega-menu", SystemName = "ManageNopStationSmartMegaMenu", Category = "NopStation" };

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
