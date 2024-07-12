using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Payments.StripePaymentElement;

public class StripePaymentElementsPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageConfiguration = new()
    {
        Name = "NopStation stripe payment element. Manage stripe payment element",
        SystemName = "ManageNopStationStripePaymentElement",
        Category = "NopStation"
    };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageConfiguration
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
                }
            )
        };
    }
}
