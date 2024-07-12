using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.CRM.Zoho.Domain;
using NopStation.Plugin.CRM.Zoho.Domain.Zoho;

namespace NopStation.Plugin.CRM.Zoho.Services
{
    public interface IMappingService
    {
        #region Stores

        Task<ZohoStore> GetZohoStoreAsync(int storeId);

        Task<IPagedList<ZohoStore>> GetZohoStoresAsync(SyncType syncType, bool update, int pageIndex = 0);
        Task<IPagedList<ZohoStore>> GetZohoStoresByStoreIdsAsync(
            IList<int> storeIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        #endregion

        #region Vendors

        Task<ZohoVendor> GetZohoVendorAsync(int vendorId);

        Task<IPagedList<ZohoVendor>> GetZohoVendorsAsync(SyncType syncType, bool update, int pageIndex = 0);

        Task<IPagedList<ZohoVendor>> GetZohoVendorsByVendorIdsAsync(
            IList<int> vendorIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        #endregion

        #region Customers

        Task<ZohoCustomer> GetZohoCustomerAsync(int customerId);

        Task<IPagedList<ZohoCustomer>> GetZohoCustomersAsync(SyncType syncType, bool update, int pageIndex = 0);

        Task<IPagedList<ZohoCustomer>> GetZohoCustomersByCustomerIdsAsync(
            IList<int> customerIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<IPagedList<Customer>> GetAllCustomersForZohoSyncAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int affiliateId = 0, int vendorId = 0, int[] customerRoleIds = null,
            string email = null, string username = null, string firstName = null, string lastName = null,
            int dayOfBirth = 0, int monthOfBirth = 0,
            string company = null, string phone = null, string zipPostalCode = null, string ipAddress = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool ignoreGuestCustomers = true);

        #endregion

        #region Products

        Task<ZohoProduct> GetZohoProductAsync(int productId);

        Task<IPagedList<ZohoProduct>> GetZohoProductsAsync(SyncType syncType, bool update, int pageIndex = 0);

        Task<IPagedList<ZohoProduct>> GetZohoProductsByProductIdsAsync(
            IList<int> productIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        #endregion

        #region Orders

        Task<ZohoOrder> GetZohoOrderAsync(int orderId);

        Task<IPagedList<ZohoOrder>> GetZohoOrdersAsync(SyncType syncType, bool update, int pageIndex = 0);

        Task<IPagedList<ZohoOrder>> GetZohoOrdersByOrderIdsAsync(
            IList<int> orderIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        #endregion

        #region Shipments

        Task<ZohoShipment> GetZohoShipmentAsync(int shipmentId);

        Task<IPagedList<ZohoShipment>> GetZohoShipmentsAsync(SyncType syncType, bool update, int pageIndex = 0);

        Task<IPagedList<ZohoShipment>> GetZohoShipmentsByShipmentIdsAsync(
            IList<int> shipmentIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        #endregion

        #region Shipment items

        Task<IPagedList<ShipmentItem>> GetAllShipmentItemsAsync(
            int? shipmentId = null,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<ZohoShipmentItem> GetZohoShipmentItemAsync(int shipmentItemId);

        Task<IPagedList<ZohoShipmentItem>> GetZohoShipmentItemsAsync(SyncType syncType, bool update, int pageIndex = 0);

        Task<IPagedList<ZohoShipmentItem>> GetZohoShipmentItemsByShipmentItemIdsAsync(
            IList<int> productIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        #endregion

        #region Data mapping

        Task InsertDataMappingAsync(List<DataMapping> dataMappings);

        Task<DataMapping> GetDataMappingByEntityIdAsync(EntityType entityType, int id);

        Task<IPagedList<DataMapping>> GetAllDataMappingAsync(
            EntityType? entityType = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        #endregion
    }
}