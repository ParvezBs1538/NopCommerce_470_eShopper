using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.UrlShortener
{
    public class UrlShortnerPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation Admin UrlShortner. Configuration", SystemName = "ManageUrlShortnerConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageUrlShortner = new PermissionRecord { Name = "NopStation UrlShortner. Manage UrlShortner", SystemName = "ManageUrlShortner", Category = "NopStation" };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageConfiguration,
                        ManageUrlShortner
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageUrlShortner
            };
        }
    }
}
