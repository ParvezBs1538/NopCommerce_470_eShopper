using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;

namespace NopStation.Plugin.SMS.BulkSms
{
    public class BulkSmsPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation Bulk SMS. Configuration", SystemName = "ManageBulkSmsConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageTemplates  = new PermissionRecord { Name = "NopStation Bulk SMS. Manage Templates", SystemName = "ManageBulkSmsTemplates", Category = "NopStation" };
        public static readonly PermissionRecord ManageQueuedSms  = new PermissionRecord { Name = "NopStation Bulk SMS. Manage Queued Notifications", SystemName = "ManageBulkSmsQueuedSms", Category = "NopStation" };

        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageConfiguration,
                        ManageTemplates,
                        ManageQueuedSms,
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageTemplates,
                ManageQueuedSms,
            };
        }
    }
}
