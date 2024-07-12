using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Payments.AamarPay;

public class AamarPayPaymentPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation AamarPayPayment. Manage AamarPayPayment", SystemName = "ManageNopStationAamarPayPayment", Category = "NopStation" };

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
