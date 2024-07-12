using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.Core;

public class CorePermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageLicense = new PermissionRecord { Name = "NopStation core. Manage license", SystemName = "ManageNopStationCoreLicense", Category = "NopStation" };
    public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation core. Manage configuration", SystemName = "ManageNopStationCoreConfiguration", Category = "NopStation" };
    public static readonly PermissionRecord ManageNopStationFeatures = new PermissionRecord { Name = "NopStation core. Manage nopstation features", SystemName = "ManageNopStationFeatures", Category = "NopStation" };
    public static readonly PermissionRecord ShowDocumentations = new PermissionRecord { Name = "NopStation core. Show Documentations", SystemName = "ShowNopStationDocumentations", Category = "NopStation" };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageLicense,
            ManageConfiguration,
            ManageNopStationFeatures,
            ShowDocumentations
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
                    ManageLicense,
                    ManageConfiguration,
                    ManageNopStationFeatures,
                    ShowDocumentations
                }
            )
        };
    }
}
