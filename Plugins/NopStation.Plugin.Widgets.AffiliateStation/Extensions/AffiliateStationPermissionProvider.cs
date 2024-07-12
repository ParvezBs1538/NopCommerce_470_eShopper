using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.AffiliateStation.Extensions
{
    public class AffiliateStationPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation affiliate station. Configure", SystemName = "ManageAffiliateStationConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageAffiliateCustomer = new PermissionRecord { Name = "NopStation affiliate station. Manage affiliate customer", SystemName = "ManageAffiliateStationCustomer", Category = "NopStation" };
        public static readonly PermissionRecord ManageCatalogCommission = new PermissionRecord { Name = "NopStation affiliate station. Manage catalog commission", SystemName = "ManageAffiliateStationCatalogCommission", Category = "NopStation" };
        public static readonly PermissionRecord ManageOrderCommission = new PermissionRecord { Name = "NopStation affiliate station. Manage order commission", SystemName = "ManageAffiliateStationOrderCommission", Category = "NopStation" };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageConfiguration,
                        ManageAffiliateCustomer,
                        ManageCatalogCommission,
                        ManageOrderCommission
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageAffiliateCustomer,
                ManageCatalogCommission,
                ManageOrderCommission
            };
        }
    }
}
