using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Extensions
{
    public class AlgoliaSearchPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "Algolia search. Configuration", SystemName = "ManageAlgoliaConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageUploadProducts = new PermissionRecord { Name = "Algolia search. Manage Upload Products", SystemName = "ManageAlgoliaUploadProducts", Category = "NopStation" };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageConfiguration,
                        ManageUploadProducts
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageUploadProducts
            };
        }
    }
}
