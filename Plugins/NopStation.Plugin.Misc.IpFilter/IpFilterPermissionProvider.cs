using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.IpFilter
{
    class IpFilterPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation IP Filter. Manage Configuration", SystemName = "ManageNopStationIpFilterConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageCountryBlockRule = new PermissionRecord { Name = "NopStation IP Filter. Manage Country Block Rules", SystemName = "ManageNopStationIpFilterCountryBlockRule", Category = "NopStation" };
        public static readonly PermissionRecord ManageIpBlockRules = new PermissionRecord { Name = "NopStation IP Filter. Manage Ip Block Rules", SystemName = "ManageNopStationIpFilterIpBlockRule", Category = "NopStation" };
        public static readonly PermissionRecord ManageCountryBlockRules = new PermissionRecord { Name = "NopStation IP Filter. Manage Ip Range Block Rules", SystemName = "ManageNopStationIpFilterIpRangeBlockRule", Category = "NopStation" };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageCountryBlockRule,
                ManageIpBlockRules,
                ManageCountryBlockRules
            };
        }

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageConfiguration,
                        ManageCountryBlockRule,
                        ManageIpBlockRules,
                        ManageCountryBlockRules
                    }
                )
            };
        }
    }
}
