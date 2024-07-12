using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.AnywhereSlider
{
    public class SliderPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageSliders = new PermissionRecord { Name = "NopStation anywhere slider. Manage slider", SystemName = "ManageNopStationSliders", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageSliders
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
                      ManageSliders
                    }
                )
            };
        }
    }
}
