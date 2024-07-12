using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.SqlManager
{
    public class SqlManagerPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageParameters = new PermissionRecord { Name = "NopStation sql manager. Manage sql parameters", SystemName = "ManageSqlManagerParameters", Category = "NopStation" };
        public static readonly PermissionRecord ManageReports = new PermissionRecord { Name = "NopStation sql manager. Manage sql reports", SystemName = "ManageSqlManagerReport", Category = "NopStation" };
        public static readonly PermissionRecord ViewReports = new PermissionRecord { Name = "NopStation sql manager. View sql reports", SystemName = "ViewSqlManagerReport", Category = "NopStation" };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageParameters,
                        ManageReports,
                        ViewReports
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageParameters,
                ManageReports,
                ViewReports
            };
        }
    }
}
