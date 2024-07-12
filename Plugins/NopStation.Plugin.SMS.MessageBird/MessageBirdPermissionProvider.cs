using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;

namespace NopStation.Plugin.SMS.MessageBird
{
    public class MessageBirdPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation MessageBird SMS. Configuration", SystemName = "ManageMessageBirdConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageTemplates  = new PermissionRecord { Name = "NopStation MessageBird SMS. Manage Templates", SystemName = "ManageMessageBirdTemplates", Category = "NopStation" };
        public static readonly PermissionRecord ManageQueuedSms  = new PermissionRecord { Name = "NopStation MessageBird SMS. Manage Queued Notifications", SystemName = "ManageMessageBirdQueuedSms", Category = "NopStation" };

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
