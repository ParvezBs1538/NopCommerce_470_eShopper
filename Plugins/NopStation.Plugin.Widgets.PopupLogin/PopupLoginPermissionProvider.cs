using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.PopupLogin
{
    public class PopupLoginPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManagePopupLogin = new PermissionRecord { Name = "NopStation Popup Login. Manage Popup Login", SystemName = "ManageNopStationPopupLogin", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManagePopupLogin
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
                        ManagePopupLogin
                    }
                )
            };
        }
    }
}
