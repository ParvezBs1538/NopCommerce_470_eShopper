using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;

namespace NopStation.Plugin.SMS.Twilio
{
    public class TwilioPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation Twilio SMS. Configuration", SystemName = "ManageTwilioConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageTemplates  = new PermissionRecord { Name = "NopStation Twilio SMS. Manage Templates", SystemName = "ManageTwilioTemplates", Category = "NopStation" };
        public static readonly PermissionRecord ManageQueuedSms  = new PermissionRecord { Name = "NopStation Twilio SMS. Manage Queued Notifications", SystemName = "ManageTwilioQueuedSms", Category = "NopStation" };

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
