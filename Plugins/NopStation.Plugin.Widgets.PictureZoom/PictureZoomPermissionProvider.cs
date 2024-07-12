using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.PictureZoom;

public class PictureZoomPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManagePictureZoom = new PermissionRecord { Name = "NopStation picture zoom. Manage picture zoom", SystemName = "ManageNopStationPictureZoom", Category = "NopStation" };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManagePictureZoom
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
                    ManagePictureZoom
                }
            )
        };
    }

}
