using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;


namespace NopStation.Plugin.Misc.QuoteCart;

public class QuoteCartPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageConfiguration = new()
    {
        Name = "NopStation QuoteCart. Manage QuoteCart Configuration",
        SystemName = "ManageNopStationQuoteCartConfiguration",
        Category = "NopStation"
    };

    public static readonly PermissionRecord ManageQuoteCartForm = new()
    {
        Name = "NopStation QuoteCart. Manage QuoteCart Form",
        SystemName = "ManageNopStationQuoteCartForm",
        Category = "NopStation"
    };

    public static readonly PermissionRecord ManageQuoteRequest = new()
    {
        Name = "NopStation QuoteCart. Manage QuoteCart Quote Request",
        SystemName = "ManageNopStationQuoteCartQuoteRequest",
        Category = "NopStation"
    };

    public static readonly PermissionRecord SendQuoteRequest = new()
    {
        Name = "NopStation QuoteCart. Send QuoteCart Quote Request",
        SystemName = "SendNopStationQuoteCartQuoteRequest",
        Category = "NopStation"
    };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return [
            ManageConfiguration,
            ManageQuoteCartForm,
            ManageQuoteRequest,
            SendQuoteRequest
        ];
    }

    public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
    {
        return new ()
        {
            (
                NopCustomerDefaults.AdministratorsRoleName,
                [
                    ManageConfiguration,
                    ManageQuoteCartForm,
                    ManageQuoteRequest
                ]
            ),
            (
                NopCustomerDefaults.RegisteredRoleName,
                [
                    SendQuoteRequest
                ]
            ),
            (
                NopCustomerDefaults.GuestsRoleName,
                [
                    SendQuoteRequest
                ]
            )
        };
    }

}
