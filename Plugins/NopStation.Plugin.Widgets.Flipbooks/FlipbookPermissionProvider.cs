using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.Flipbooks
{
    public class FlipbookPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation flipbooks. Configuration", SystemName = "ManageNopStationFlipbooksConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageFlipbooks = new PermissionRecord { Name = "NopStation flipbooks. Manage flipbooks", SystemName = "ManageNopStationFlipbooks", Category = "NopStation" };
        
        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageConfiguration,
                        ManageFlipbooks
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageFlipbooks
            };
        }
    }
}
