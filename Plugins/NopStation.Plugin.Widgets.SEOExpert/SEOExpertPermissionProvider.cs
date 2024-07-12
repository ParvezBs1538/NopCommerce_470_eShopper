using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.SEOExpert
{
    public class SEOExpertPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageSEOExpert = new PermissionRecord { Name = "NopStation seo expert. Manage seo expert", SystemName = "ManageNopStationSEOExpert", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageSEOExpert
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
                        ManageSEOExpert
                    }
                )
            };
        }
    }
}
