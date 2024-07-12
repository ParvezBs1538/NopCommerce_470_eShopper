using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.VendorShop
{
    public class VendorShopPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new() { Name = "NopStation VendorShop. Manage VendorShop", SystemName = "ManageNopStationVendorShop", Category = "NopStation" };
        public static readonly PermissionRecord ManageOCarousels = new() { Name = "NopStation VendorShop OCarousel. Manage carousels", SystemName = "ManageNopStationVendorShopOCarousels", Category = "NopStation" };
        public static readonly PermissionRecord ManageSliders = new() { Name = "NopStation VendorShop anywhere slider. Manage slider", SystemName = "ManageNopStationVendorShopSliders", Category = "NopStation" };
        public static readonly PermissionRecord ManageVendorProfile = new() { Name = "NopStation VendorShop vendor profile. Manage profile", SystemName = "ManageNopStationVendorShopVendorProfile", Category = "NopStation" };
        public static readonly PermissionRecord ManageProductTab = new() { Name = "NopStation VendorShop product tabs. Manage product tabs", SystemName = "ManageNopStationVendorShopProductTab", Category = "NopStation" };
        public static readonly PermissionRecord ManageSubscriber = new() { Name = "NopStation VendorShop subscriber . Manage subscriber", SystemName = "ManageNopStationVendorShopSubscriber", Category = "NopStation" };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageConfiguration,
                    }
                ),
                (
                    NopCustomerDefaults.VendorsRoleName,
                    new []
                    {
                        ManageVendorProfile,
                        ManageOCarousels,
                        ManageSliders,
                        ManageProductTab,
                        ManageSubscriber
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageOCarousels,
                ManageSliders,
                ManageVendorProfile,
                ManageProductTab,
                ManageSubscriber
            };
        }
    }
}
