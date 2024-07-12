using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.GoogleTagManager
{
    public class GoogleTagManagerPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation Google Tag Manager. Manage Configuration", SystemName = "ManageNopStationGoogleTagManagerConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageExportFile = new PermissionRecord { Name = "NopStation Google Tag Manager. Manage ExportFile", SystemName = "ManageNopStationGoogleTagManagerExportFile", Category = "NopStation" };
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageExportFile
            };
        }
        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {

            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                      ManageConfiguration,
                      ManageExportFile
                    }
                )
            };
        }
    }
}