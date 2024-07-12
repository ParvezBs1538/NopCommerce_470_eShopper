using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.AdminReportExporter
{
    public class AdminReportExporterPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation Admin Report Exporter. Configuration", SystemName = "ManageAdminReportExporterConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageAdminReportExporter = new PermissionRecord { Name = "NopStation Admin Report Exporter. Manage AdminReportExporter", SystemName = "ManageAdminReportExporter", Category = "NopStation" };

        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageConfiguration,
                        ManageAdminReportExporter
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageAdminReportExporter
            };
        }
    }
}
