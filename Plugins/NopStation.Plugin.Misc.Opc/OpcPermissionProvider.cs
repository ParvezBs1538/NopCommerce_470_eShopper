using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.Opc;

public class OpcPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageOpc = new PermissionRecord { Name = "NopStation One Page Checkout. Manage One Page Checkout", SystemName = "ManageNopStationOpc", Category = "NopStation" };

    public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
    {
        return new HashSet<(string, PermissionRecord[])>
        {
            (
                NopCustomerDefaults.AdministratorsRoleName,
                new[]
                {
                    ManageOpc
                }
            )
        };
    }

    public IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageOpc
        };
    }
}