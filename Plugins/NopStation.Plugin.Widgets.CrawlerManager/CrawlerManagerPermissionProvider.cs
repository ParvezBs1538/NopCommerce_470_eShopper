using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.CrawlerManager
{
    public class CrawlerManagerPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageCrawlerManager = new PermissionRecord { Name = "NopStation Crawler Manager. Manage Crawlers list", SystemName = "ManageNopStationCrawlerManager", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageCrawlerManager,
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
                        ManageCrawlerManager,
                    }
                )
            };
        }
    }
}
