using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.AjaxFilter
{
    public class AjaxFilterPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageAjaxFilter = new PermissionRecord { Name = "NopStation ajax filter. Manage filters", SystemName = "ManageNopStationAjaxFilter", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageAjaxFilter
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
                      ManageAjaxFilter
                    }
                )
            };
        }
    }
}
