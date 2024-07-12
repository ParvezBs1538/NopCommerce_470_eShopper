using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.Helpdesk
{
    public class HelpdeskPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageTickets = new PermissionRecord { Name = "NopStation helpdesk. Manage Tickets", SystemName = "NopStationHelpdeskManageTickets", Category = "NopStation" };
        public static readonly PermissionRecord ManageCategories = new PermissionRecord { Name = "NopStation helpdesk. Manage Categories", SystemName = "NopStationHelpdeskManageCategories", Category = "NopStation" };
        public static readonly PermissionRecord ManageStaffs = new PermissionRecord { Name = "NopStation helpdesk. Manage Staffs", SystemName = "NopStatioHelpdesknManageStaffs", Category = "NopStation" };
        public static readonly PermissionRecord ManageDepartments = new PermissionRecord { Name = "NopStation helpdesk. Manage Departments", SystemName = "NopStatioHelpdesknManageDepartments", Category = "NopStation" };
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation helpdesk. Manage Configuration", SystemName = "NopStationHelpdeskManageConfiguration", Category = "NopStation" };

        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageTickets,
                        ManageCategories,
                        ManageStaffs,
                        ManageDepartments,
                        ManageConfiguration
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageTickets,
                ManageCategories,
                ManageStaffs,
                ManageDepartments,
                ManageConfiguration
            };
        }
    }
}