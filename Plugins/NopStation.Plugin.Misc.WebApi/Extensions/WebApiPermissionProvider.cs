using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.WebApi.Extensions
{
    public class WebApiPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation Web api. Configuration", SystemName = "ManageWebApiConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageSlider = new PermissionRecord { Name = "NopStation Web api. Manage slider", SystemName = "ManageWebApiSlider", Category = "NopStation" };
        public static readonly PermissionRecord ManageCategoryIcon = new PermissionRecord { Name = "NopStation Web api. Manage category icon", SystemName = "ManageWebApiCategoryIcon", Category = "NopStation" };
        public static readonly PermissionRecord ManageDevice = new PermissionRecord { Name = "NopStation Web api. Manage device", SystemName = "ManageWebApiDevice", Category = "NopStation" };


        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                       NopCustomerDefaults.AdministratorsRoleName,
                        new[]
                        {
                             ManageConfiguration,
                             ManageSlider,
                             ManageCategoryIcon,
                             ManageDevice
                        }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageSlider,
                ManageCategoryIcon,
                ManageDevice
            };
        }


    }
}
