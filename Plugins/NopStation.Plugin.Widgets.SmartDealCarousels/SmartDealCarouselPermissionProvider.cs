using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.SmartDealCarousels;

public class SmartDealCarouselPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageSmartDealCarousels = new PermissionRecord { Name = "NopStation SmartDealCarousel. Manage carousels", SystemName = "ManageNopStationSmartDealCarousels", Category = "NopStation" };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageSmartDealCarousels
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
                    ManageSmartDealCarousels
                }
            )
        };
    }
}
