using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.DeliveryScheduler
{
    public class DeliverySchedulerPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation Delivery Scheduler. Manage Configuration", SystemName = "ManageNopStationDeliverySchedulerConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageDeliverySlots = new PermissionRecord { Name = "NopStation Delivery Scheduler. Manage Delivery Slots", SystemName = "ManageNopStationDeliverySchedulerSlots", Category = "NopStation" };
        public static readonly PermissionRecord ManageDeliveryCapacity = new PermissionRecord { Name = "NopStation Delivery Scheduler. Manage Delivery Capacity", SystemName = "ManageNopStationDeliverySchedulerCapacity", Category = "NopStation" };
        public static readonly PermissionRecord ViewOrderDeliveryInfo = new PermissionRecord { Name = "NopStation Delivery Scheduler. View Order Delivery Info", SystemName = "ViewNopStationDeliverySchedulerDeliveryInfo", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageDeliverySlots,
                ManageDeliveryCapacity,
                ViewOrderDeliveryInfo
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
                        ManageConfiguration,
                        ManageDeliverySlots,
                        ManageDeliveryCapacity,
                        ViewOrderDeliveryInfo
                    }
                )
            };
        }
    }
}
