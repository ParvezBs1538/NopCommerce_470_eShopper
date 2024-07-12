using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.SmartCarousels;

public class SmartCarouselPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageSmartCarousels = new PermissionRecord { Name = "NopStation SmartCarousel. Manage carousels", SystemName = "ManageNopStationSmartCarousels", Category = "NopStation" };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageSmartCarousels
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
                    ManageSmartCarousels
                }
            )
        };
    }
}
