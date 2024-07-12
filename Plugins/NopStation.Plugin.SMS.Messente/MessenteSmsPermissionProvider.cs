using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;

namespace NopStation.Plugin.SMS.Messente
{
    public class MessenteSmsPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation Messente SMS. Configuration", SystemName = "ManageMessenteSmsConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageTemplates  = new PermissionRecord { Name = "NopStation Messente SMS. Manage Templates", SystemName = "ManageMessenteSmsTemplates", Category = "NopStation" };
        public static readonly PermissionRecord ManageQueuedSms  = new PermissionRecord { Name = "NopStation Messente SMS. Manage Queued Notifications", SystemName = "ManageMessenteSmsQueuedSms", Category = "NopStation" };

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
