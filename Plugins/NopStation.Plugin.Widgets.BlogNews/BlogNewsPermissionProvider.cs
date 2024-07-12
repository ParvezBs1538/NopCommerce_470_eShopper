using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widget.BlogNews;

public class BlogNewsPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageBlogNews = new PermissionRecord { Name = "NopStation blog news. Manage blog news", SystemName = "ManageNopStationBlogNews", Category = "NopStation" };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageBlogNews
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
                    ManageBlogNews
                }
            )
        };
    }

}
