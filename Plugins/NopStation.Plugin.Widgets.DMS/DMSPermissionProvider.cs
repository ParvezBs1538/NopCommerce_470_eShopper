using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.DMS
{
    public class DMSPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation delievery management system. Manage DMS configuration", SystemName = "ManageNopStationDMSConfiguration", Category = "NopStation" };
        public static readonly PermissionRecord ManageShipper = new PermissionRecord { Name = "NopStation delievery management system. Manage shippers", SystemName = "ManageNopStationShipper", Category = "NopStation" };
        public static readonly PermissionRecord ManageCourierShipment = new PermissionRecord { Name = "NopStation delievery management system. Manage courier shipments", SystemName = "ManageNopStationCourierShipments", Category = "NopStation" };
        public static readonly PermissionRecord ManageShipmentPickupPoint = new PermissionRecord { Name = "NopStation delievery management system. Manage courier shipment pickup point", SystemName = "ManageNopStationShipmentPickupPoint", Category = "NopStation" };
        public static readonly PermissionRecord ManageDevice = new PermissionRecord { Name = "NopStation DMS. Manage shipper device", SystemName = "ManageDMSShipperDevice", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageShipper,
                ManageCourierShipment,
                ManageShipmentPickupPoint,
                ManageDevice
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
                        ManageShipper,
                        ManageCourierShipment,
                        ManageShipmentPickupPoint,
                        ManageDevice
                    }
                )
            };
        }
    }
}
