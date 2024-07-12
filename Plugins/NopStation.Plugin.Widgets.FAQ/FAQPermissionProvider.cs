using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.FAQ
{
    public class FAQPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation FAQ. Manage FAQ", SystemName = "ManageNopStationFAQ", Category = "NopStation" };
        public static readonly PermissionRecord ManageItems = new PermissionRecord { Name = "NopStation FAQ. Manage Items", SystemName = "ManageNopStationFAQItems", Category = "NopStation" };
        public static readonly PermissionRecord ManageCategories = new PermissionRecord { Name = "NopStation FAQ. Manage Categories", SystemName = "ManageNopStationFAQCategories", Category = "NopStation" };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageConfiguration,
                        ManageItems,
                        ManageCategories
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageItems,
                ManageCategories
            };
        }
    }
}
