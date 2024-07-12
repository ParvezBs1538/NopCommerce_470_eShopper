using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.DynamicSurvey
{
    public class DynamicSurveyPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageDynamicSurvey = new PermissionRecord { Name = "NopStation Dynamic Survey. Manage Dynamic Survey", SystemName = "ManageNopStationDynamicSurvey", Category = "NopStation" };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageDynamicSurvey,
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageDynamicSurvey,
            };
        }
    }
}
