using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;

namespace NopStation.Plugin.SMS.TeleSign
{
    public class TeleSignSmsPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation TeleSign SMS. Configuration", SystemName = "ManageTeleSignConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageTemplates  = new PermissionRecord { Name = "NopStation TeleSign SMS. Manage Templates", SystemName = "ManageTeleSignTemplates", Category = "NopStation" };
        public static readonly PermissionRecord ManageQueuedSms  = new PermissionRecord { Name = "NopStation TeleSign SMS. Manage Queued Notifications", SystemName = "ManageTeleSignQueuedSms", Category = "NopStation" };

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
