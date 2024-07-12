using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.Announcement;

public class AnnouncementPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageAnnouncement = new PermissionRecord { Name = "NopStation announcement. Manage announcement", SystemName = "ManageNopStationAnnouncement", Category = "NopStation" };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageAnnouncement
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
                    ManageAnnouncement,
                }
            )
        };
    }
}
