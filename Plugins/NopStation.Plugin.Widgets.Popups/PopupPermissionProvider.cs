using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.Popups;

public class PopupPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManagePopup = new PermissionRecord { Name = "NopStation popup. Manage popup", SystemName = "ManageNopStationPopup", Category = "NopStation" };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManagePopup
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
                    ManagePopup
                }
            )
        };
    }
}
