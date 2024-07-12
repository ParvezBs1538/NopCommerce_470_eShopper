using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.OCarousels
{
    public class OCarouselPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageOCarousels = new PermissionRecord { Name = "NopStation OCarousel. Manage carousels", SystemName = "ManageNopStationOCarousels", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageOCarousels
            };
        }

        HashSet<(string systemRoleName, PermissionRecord[] permissions)> IPermissionProvider.GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageOCarousels
                    }
                )
            };
        }
    }
}
