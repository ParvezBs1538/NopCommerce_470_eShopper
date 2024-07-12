using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using NopStation.Plugin.CRM.Zoho.Domain;
using NopStation.Plugin.CRM.Zoho.Domain.Zoho;
using NopStation.Plugin.CRM.Zoho.Helpers;
using NopStation.Plugin.CRM.Zoho.Services.Zoho;
using NopStation.Plugin.CRM.Zoho.Services.Zoho.Request;
using NopStation.Plugin.CRM.Zoho.Services.Zoho.Response;
using RestSharp;

namespace NopStation.Plugin.CRM.Zoho.Services
{
    public class ZohoService : IZohoService
    {
        #region Fields

        private readonly int _pageSize = 100;
        private readonly ICustomerService _customerService;
        private readonly ILogger _logger;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IShipmentService _shipmentService;
        private readonly IWebHelper _webHelper;
        private readonly ZohoCRMSettings _zohoCRMSettings;
        private readonly IMappingService _zohoDataMappingService;
        private readonly ISettingService _settingService;
        private readonly ZohoCRMShipmentSettings _zohoCRMShipmentSettings;
        private readonly IUpdatableItemService _updatableItemService;

        #endregion

        #region Ctor

        public ZohoService(
            ICustomerService customerService,
            ILogger logger,
            IProductService productService,
            IOrderService orderService,
            IStoreService storeService,
            IVendorService vendorService,
            IShipmentService shipmentService,
            IWebHelper webHelper,
            ZohoCRMSettings zohoCRMSettings,
            IMappingService zohoDataMappingService,
            ISettingService settingService,
            ZohoCRMShipmentSettings zohoCRMShipmentSettings,
            IUpdatableItemService updatableItemService)
        {
            _customerService = customerService;
            _logger = logger;
            _productService = productService;
            _orderService = orderService;
            _storeService = storeService;
            _vendorService = vendorService;
            _shipmentService = shipmentService;
            _webHelper = webHelper;
            _zohoCRMSettings = zohoCRMSettings;
            _zohoDataMappingService = zohoDataMappingService;
            _settingService = settingService;
            _zohoCRMShipmentSettings = zohoCRMShipmentSettings;
            _updatableItemService = updatableItemService;
        }

        #endregion

        #region Utilities

        protected string GetAuthRedirectUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/ZohoCRM/Authorize";
        }

        protected async Task<IRestResponse> GetResponseAsync(string url, BaseZohoParentType entity, Method method)
        {
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(method);
            request.AddHeader("Authorization", "Zoho-oauthtoken " + _zohoCRMSettings.AccessToken);
            request.AddHeader("Content-Type", "application/json");

            if (entity != null)
            {
                var body = JsonConvert.SerializeObject(entity);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
            }
            var response = await client.ExecuteAsync(request);

            return response;
        }

        protected async Task<CommonResponse> SyncAccountsAsync(IList<ZohoStore> stores, Method method)
        {
            var accountsRequest = new AccountsRequest();
            foreach (var store in stores)
                accountsRequest.Data.Add(await store.ToZohoAccountAsync());

            var response = await GetResponseAsync(string.Format(ZohoDefaults.AccountsUrl, GetApiUrl()), accountsRequest, method);
            return JsonConvert.DeserializeObject<CommonResponse>(response.Content);
        }

        protected async Task<CommonResponse> SyncVendorsAsync(IList<ZohoVendor> vendors, Method method)
        {
            var vendorsRequest = new VendorsRequest();
            foreach (var vendor in vendors)
                vendorsRequest.Data.Add(await vendor.ToZohoVendorAsync());

            var response = await GetResponseAsync(string.Format(ZohoDefaults.VendorsUrl, GetApiUrl()), vendorsRequest, method);
            return JsonConvert.DeserializeObject<CommonResponse>(response.Content);
        }

        protected async Task<CommonResponse> SyncContactsAsync(IList<ZohoCustomer> customers, Method method)
        {
            var contactsRequest = new ContactsRequest();
            foreach (var customer in customers)
                contactsRequest.Data.Add(await customer.ToZohoContactAsync());

            var response = await GetResponseAsync(string.Format(ZohoDefaults.ContactsUrl, GetApiUrl()), contactsRequest, method);
            return JsonConvert.DeserializeObject<CommonResponse>(response.Content);
        }

        protected async Task<CommonResponse> SyncProductsAsync(IList<ZohoProduct> products, Method method)
        {
            var productsRequest = new ProductsRequest();
            foreach (var product in products)
                productsRequest.Data.Add(await product.ToZohoProductAsync());

            var response = await GetResponseAsync(string.Format(ZohoDefaults.ProductsUrl, GetApiUrl()), productsRequest, method);
            return JsonConvert.DeserializeObject<CommonResponse>(response.Content);
        }

        protected async Task<CommonResponse> SyncOrdersAsync(IList<ZohoOrder> orders, Method method)
        {
            var ordersRequest = new SalesOrdersRequest();
            foreach (var order in orders)
                ordersRequest.Data.Add(await order.ToZohoSalesOrderAsync());

            var response = await GetResponseAsync(string.Format(ZohoDefaults.SalesOrdersUrl, GetApiUrl()), ordersRequest, method);
            return JsonConvert.DeserializeObject<CommonResponse>(response.Content);
        }

        protected async Task<CommonResponse> SyncShipmentsAsync(IList<ZohoShipment> shipments, Method method)
        {
            var shipmentsRequest = new ShipmentsRequest();
            foreach (var shipment in shipments)
                shipmentsRequest.Data.Add(await shipment.ToZohoShipmentAsync(_zohoCRMShipmentSettings));

            var url = GetApiUrl() + _zohoCRMSettings.ShipmentModuleName;
            var response = await GetResponseAsync(url, shipmentsRequest, method);
            return JsonConvert.DeserializeObject<CommonResponse>(response.Content);
        }

        protected async Task<CommonResponse> SyncShipmentItemsAsync(IList<ZohoShipmentItem> shipmentItems, Method method)
        {
            var shipmentItemsRequest = new ShipmentItemsRequest();
            foreach (var shipment in shipmentItems)
                shipmentItemsRequest.Data.Add(await shipment.ToZohoShipmentItemAsync(_zohoCRMShipmentSettings));

            var url = GetApiUrl() + _zohoCRMSettings.ShipmentItemModuleName;
            var response = await GetResponseAsync(url, shipmentItemsRequest, method);
            return JsonConvert.DeserializeObject<CommonResponse>(response.Content);
        }

        protected string GetAuthUrl()
        {
            return _zohoCRMSettings.DataCenterId switch
            {
                (int)DataCenter.Australia => "https://accounts.zoho.com.au/",
                (int)DataCenter.Europe => "https://accounts.zoho.eu/",
                (int)DataCenter.India => "https://accounts.zoho.in/",
                (int)DataCenter.China => "https://accounts.zoho.com.cn/",
                (int)DataCenter.Japan => "https://accounts.zoho.jp/",
                _ => "https://accounts.zoho.com/",
            };
        }

        protected string GetApiUrl()
        {
            if (_zohoCRMSettings.UseSandbox)
            {
                return _zohoCRMSettings.DataCenterId switch
                {
                    (int)DataCenter.Australia => "https://sandbox.zohoapis.com.au/crm/v2/",
                    (int)DataCenter.Europe => "https://sandbox.zohoapis.eu/crm/v2/",
                    (int)DataCenter.India => "https://sandbox.zohoapis.in/crm/v2/",
                    (int)DataCenter.China => "https://sandbox.zohoapis.com.cn/crm/v2/",
                    (int)DataCenter.Japan => "https://sandbox.zohoapis.jp/crm/v2/",
                    _ => "https://sandbox.zohoapis.com/crm/v2/",
                };
            }

            return _zohoCRMSettings.DataCenterId switch
            {
                (int)DataCenter.Australia => "https://zohoapis.com.au/crm/v2/",
                (int)DataCenter.Europe => "https://zohoapis.eu/crm/v2/",
                (int)DataCenter.India => "https://zohoapis.in/crm/v2/",
                (int)DataCenter.China => "https://zohoapis.com.cn/crm/v2/",
                (int)DataCenter.Japan => "https://zohoapis.jp/crm/v2/",
                _ => "https://zohoapis.com/crm/v2/",
            };
        }

        #endregion

        #region Methods

        #region Auth

        public string GetAuthorizationUrl(string clientId)
        {
            return string.Format("{0}oauth/{1}/auth?scope={2}&client_id={3}&response_type=code&access_type=offline&redirect_uri={4}",
                GetAuthUrl(), ZohoDefaults.OAuthApiVersion, ZohoDefaults.Scopes, clientId, GetAuthRedirectUrl());
        }

        public async Task<OAuthResponse> AuthorizeAccessTokenAsync(string code)
        {
            var client = new RestClient(string.Format(ZohoDefaults.TokenUrl, GetAuthUrl()));
            client.Timeout = -1;

            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("client_id", _zohoCRMSettings.ClientId);
            request.AddParameter("client_secret", _zohoCRMSettings.ClientSecret);
            request.AddParameter("redirect_uri", GetAuthRedirectUrl());
            request.AddParameter("code", code);
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("prompt", "consent");
            request.AddParameter("access_type", "offline");
            var response = await client.ExecuteAsync(request);

            return JsonConvert.DeserializeObject<OAuthResponse>(response.Content);
        }

        public async Task<OAuthResponse> RefreshAccessTokenAsync()
        {
            var client = new RestClient(string.Format(ZohoDefaults.TokenUrl, GetAuthUrl()));
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("client_id", _zohoCRMSettings.ClientId);
            request.AddParameter("client_secret", _zohoCRMSettings.ClientSecret);
            request.AddParameter("refresh_token", _zohoCRMSettings.RefreshToken);
            request.AddParameter("grant_type", "refresh_token");
            var response = await client.ExecuteAsync(request);

            var authResponse = JsonConvert.DeserializeObject<OAuthResponse>(response.Content);

            _zohoCRMSettings.AccessToken = authResponse.AccessToken;
            _zohoCRMSettings.ExpireOnUtc = DateTime.UtcNow.AddSeconds(authResponse.ExpiresIn);
            await _settingService.SaveSettingAsync(_zohoCRMSettings);

            return authResponse;
        }

        public async Task<ModulesResponse> GetModulesAsync()
        {
            try
            {
                if (_zohoCRMSettings.ExpireOnUtc.AddMinutes(-1) <= DateTime.UtcNow)
                    await RefreshAccessTokenAsync();

                var response = await GetResponseAsync(string.Format(ZohoDefaults.ModulesUrl, GetApiUrl()), null, Method.GET);
                return JsonConvert.DeserializeObject<ModulesResponse>(response.Content);
            }
            catch (Exception ex)
            {
                return new ModulesResponse() { Error = ex.Message };
            }
        }

        public async Task<ModuleFieldsResponse> GetModuleFieldsAsync(string moduleName)
        {
            try
            {
                var url = string.Format("{0}settings/fields?module={1}", GetApiUrl(), moduleName);
                var response = await GetResponseAsync(url, null, Method.GET);
                return JsonConvert.DeserializeObject<ModuleFieldsResponse>(response.Content);
            }
            catch (Exception ex)
            {
                return new ModuleFieldsResponse() { Error = ex.Message };
            }
        }

        #endregion

        #region Sync

        #region Store

        public async Task SyncStoresOnZohoAsync(IList<int> storeIds, SyncType syncType = SyncType.DifferentialSync)
        {
            if (storeIds == null || !storeIds.Any())
                throw new ArgumentNullException(nameof(storeIds));

            var newStores = await _zohoDataMappingService.GetZohoStoresByStoreIdsAsync(storeIds, false);
            var existingStores = await _zohoDataMappingService.GetZohoStoresByStoreIdsAsync(storeIds, true);

            if (newStores != null && newStores.Any())
            {
                if (newStores.Count < 100)
                    Console.WriteLine(newStores.Count);

                var response = await SyncAccountsAsync(newStores, Method.POST);

                var maps = new List<DataMapping>();
                var deleteIds = new List<int>();

                for (var i = 0; i < response.Data.Count; i++)
                {
                    var datum = response.Data[i];
                    if (string.IsNullOrWhiteSpace(datum.Details.Id))
                    {
                        await _logger.ErrorAsync($"Sync Error for Store, Code: {datum.Code}, Status: {datum.Status}.");
                        continue;
                    }

                    maps.Add(new DataMapping
                    {
                        ZohoId = datum.Details.Id,
                        EntityTypeId = (int)EntityType.Stores,
                        EntityId = newStores[i].Id
                    });

                    if (datum.Message == "SUCCESS")
                        deleteIds.Add(newStores[i].Id);
                }

                await _zohoDataMappingService.InsertDataMappingAsync(maps);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Stores, deleteIds);
            }
            if (existingStores != null && existingStores.Any())
            {
                await SyncAccountsAsync(existingStores, Method.PUT);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Stores, existingStores.Select(ec => ec.Id).ToList());
            }
        }

        public async Task SyncStoresAsync(SyncType syncType)
        {
            var pageIndex = 0;
            if (syncType == SyncType.FullSync)
            {
                var nopStores = await _storeService.GetAllStoresAsync();
                if (nopStores == null || !nopStores.Any())
                    return;

                var nopStoreIds = nopStores.Select(s => s.Id).ToList();
                while (true)
                {
                    var nopStoreIdSubList = await nopStoreIds.Skip(pageIndex * _pageSize).Take(_pageSize).ToListAsync();
                    if (nopStoreIdSubList == null || nopStoreIdSubList.Count == 0)
                        break;
                    await SyncStoresOnZohoAsync(nopStoreIdSubList, syncType);
                    pageIndex++;
                }
            }
            else if (syncType == SyncType.DifferentialSync)
            {
                while (true)
                {
                    var updatableItems = await _updatableItemService.GetAllUpdatableItemAsync(EntityType.Stores, pageIndex: pageIndex, pageSize: _pageSize);
                    if (updatableItems == null || !updatableItems.Any())
                        break;
                    var nopStoreIds = updatableItems.Select(s => s.Id).ToList();
                    await SyncStoresOnZohoAsync(nopStoreIds);
                    pageIndex++;
                }
                await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Stores);
            }
            await _logger.InformationAsync($"Sync Done for stores. SyncType: {syncType}");
        }

        #endregion

        #region Vendor

        public async Task SyncVendorsOnZohoAsync(IList<int> vendorIds, SyncType syncType = SyncType.DifferentialSync)
        {
            if (vendorIds == null || !vendorIds.Any())
                throw new ArgumentNullException(nameof(vendorIds));

            var newVendors = await _zohoDataMappingService.GetZohoVendorsByVendorIdsAsync(vendorIds, false);
            var existingVendors = await _zohoDataMappingService.GetZohoVendorsByVendorIdsAsync(vendorIds, true);

            if (newVendors != null && newVendors.Any())
            {
                if (newVendors.Count < 100)
                    Console.WriteLine(newVendors.Count);

                var response = await SyncVendorsAsync(newVendors, Method.POST);

                var maps = new List<DataMapping>();
                var deleteIds = new List<int>();

                for (var i = 0; i < response.Data.Count; i++)
                {
                    var datum = response.Data[i];
                    if (string.IsNullOrWhiteSpace(datum.Details.Id))
                    {
                        await _logger.ErrorAsync($"Sync Error for Vendor, Code: {datum.Code}, Status: {datum.Status}.");
                        continue;
                    }

                    maps.Add(new DataMapping
                    {
                        ZohoId = datum.Details.Id,
                        EntityTypeId = (int)EntityType.Vendors,
                        EntityId = newVendors[i].Id
                    });

                    if (datum.Message == "SUCCESS")
                        deleteIds.Add(newVendors[i].Id);
                }

                await _zohoDataMappingService.InsertDataMappingAsync(maps);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Vendors, deleteIds);
            }
            if (existingVendors != null && existingVendors.Any())
            {
                await SyncVendorsAsync(existingVendors, Method.PUT);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Vendors, existingVendors.Select(ec => ec.Id).ToList());
            }
        }

        public async Task SyncVendorsAsync(SyncType syncType)
        {
            var pageIndex = 0;
            if (syncType == SyncType.FullSync)
            {
                while (true)
                {
                    var nopVendors = await _vendorService.GetAllVendorsAsync(pageIndex: pageIndex, pageSize: _pageSize, showHidden: true);
                    if (nopVendors == null || !nopVendors.Any())
                        break;
                    var nopVendorIds = nopVendors.Select(c => c.Id).ToList();
                    await SyncVendorsOnZohoAsync(nopVendorIds, syncType);
                    pageIndex++;
                }
            }
            else if (syncType == SyncType.DifferentialSync)
            {
                while (true)
                {
                    var updatableItems = await _updatableItemService.GetAllUpdatableItemAsync(EntityType.Vendors, pageIndex: pageIndex, pageSize: _pageSize);
                    if (updatableItems == null || !updatableItems.Any())
                        break;
                    var nopVendorIds = updatableItems.Select(c => c.Id).ToList();
                    await SyncVendorsOnZohoAsync(nopVendorIds);
                    pageIndex++;
                }

                await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Vendors);
            }

            await _logger.InformationAsync($"Sync Done for Vendor. SyncType: {syncType}");
        }

        #endregion

        #region Customer

        public async Task SyncCustomersOnZohoAsync(IList<int> customerIds, SyncType syncType = SyncType.DifferentialSync)
        {
            if (customerIds == null || !customerIds.Any())
                throw new ArgumentNullException(nameof(customerIds));

            var newCustomers = await _zohoDataMappingService.GetZohoCustomersByCustomerIdsAsync(customerIds, false);
            var existingCustomers = await _zohoDataMappingService.GetZohoCustomersByCustomerIdsAsync(customerIds, true);

            if (newCustomers != null && newCustomers.Any())
            {
                if (newCustomers.Count < 100)
                    Console.WriteLine(newCustomers.Count);

                var response = await SyncContactsAsync(newCustomers, Method.POST);

                var maps = new List<DataMapping>();
                var deleteIds = new List<int>();

                for (var i = 0; i < response.Data.Count; i++)
                {
                    var datum = response.Data[i];
                    if (string.IsNullOrWhiteSpace(datum.Details.Id))
                    {
                        await _logger.ErrorAsync($"Sync Error for Customer, Code: {datum.Code}, Status: {datum.Status}.");
                        continue;
                    }

                    maps.Add(new DataMapping
                    {
                        ZohoId = datum.Details.Id,
                        EntityTypeId = (int)EntityType.Customers,
                        EntityId = newCustomers[i].Id
                    });

                    if (datum.Message == "SUCCESS")
                        deleteIds.Add(newCustomers[i].Id);
                }

                await _zohoDataMappingService.InsertDataMappingAsync(maps);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Customers, deleteIds);
            }
            if (existingCustomers != null && existingCustomers.Any())
            {
                await SyncContactsAsync(existingCustomers, Method.PUT);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Customers, existingCustomers.Select(ec => ec.Id).ToList());
            }
        }

        public async Task SyncCustomersAsync(SyncType syncType)
        {
            var pageIndex = 0;
            if (syncType == SyncType.FullSync)
            {
                while (true)
                {
                    var nopCustomers = await _zohoDataMappingService.GetAllCustomersForZohoSyncAsync(pageIndex: pageIndex, pageSize: _pageSize);
                    if (nopCustomers == null || !nopCustomers.Any())
                        break;
                    var nopCustomerIds = nopCustomers.Select(c => c.Id).ToList();
                    await SyncCustomersOnZohoAsync(nopCustomerIds, syncType);
                    pageIndex++;
                }
            }
            else if (syncType == SyncType.DifferentialSync)
            {
                while (true)
                {
                    var updatableItems = await _updatableItemService.GetAllUpdatableItemAsync(EntityType.Customers, pageIndex: pageIndex, pageSize: _pageSize);
                    if (updatableItems == null || !updatableItems.Any())
                        break;
                    var nopCustomerIds = updatableItems.Select(c => c.Id).ToList();
                    await SyncCustomersOnZohoAsync(nopCustomerIds);
                    pageIndex++;
                }

                await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Customers);
            }

            await _logger.InformationAsync($"Sync Done for Customer. SyncType: {syncType.ToString()}.");
        }

        #endregion

        #region Product

        public async Task SyncProductsOnZohoAsync(IList<int> productIds, SyncType syncType = SyncType.DifferentialSync)
        {
            if (productIds == null || !productIds.Any())
                throw new ArgumentNullException(nameof(productIds));

            var newProducts = await _zohoDataMappingService.GetZohoProductsByProductIdsAsync(productIds, false);
            var existingProducts = await _zohoDataMappingService.GetZohoProductsByProductIdsAsync(productIds, true);

            if (newProducts != null && newProducts.Any())
            {
                if (newProducts.Count < 100)
                    Console.WriteLine(newProducts.Count);

                var response = await SyncProductsAsync(newProducts, Method.POST);

                var maps = new List<DataMapping>();
                var deleteIds = new List<int>();

                for (var i = 0; i < response.Data.Count; i++)
                {
                    var datum = response.Data[i];
                    if (string.IsNullOrWhiteSpace(datum.Details.Id))
                    {
                        await _logger.ErrorAsync($"Sync Error for Product, Code: {datum.Code}, Status: {datum.Status}.");
                        continue;
                    }

                    maps.Add(new DataMapping
                    {
                        ZohoId = datum.Details.Id,
                        EntityTypeId = (int)EntityType.Products,
                        EntityId = newProducts[i].Id
                    });

                    if (datum.Message == "SUCCESS")
                        deleteIds.Add(newProducts[i].Id);
                }

                await _zohoDataMappingService.InsertDataMappingAsync(maps);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Products, deleteIds);
            }
            if (existingProducts != null && existingProducts.Any())
            {
                await SyncProductsAsync(existingProducts, Method.PUT);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Products, existingProducts.Select(ec => ec.Id).ToList());
            }
        }

        public async Task SyncProductsAsync(SyncType syncType)
        {
            var pageIndex = 0;
            if (syncType == SyncType.FullSync)
            {
                while (true)
                {
                    var nopProducts = await _productService.SearchProductsAsync(showHidden: true, pageIndex: pageIndex, pageSize: _pageSize);
                    if (nopProducts == null || !nopProducts.Any())
                        break;
                    var nopProductIds = nopProducts.Select(c => c.Id).ToList();
                    await SyncProductsOnZohoAsync(nopProductIds, syncType);
                    pageIndex++;
                }
            }
            else if (syncType == SyncType.DifferentialSync)
            {
                while (true)
                {
                    var updatableItems = await _updatableItemService.GetAllUpdatableItemAsync(EntityType.Products, pageIndex: pageIndex, pageSize: _pageSize);
                    if (updatableItems == null || !updatableItems.Any())
                        break;
                    var nopProductIds = updatableItems.Select(c => c.Id).ToList();
                    await SyncProductsOnZohoAsync(nopProductIds);
                    pageIndex++;
                }

                await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Products);
            }

            await _logger.InformationAsync($"Sync Done for Product. SyncType: {syncType}.");
        }


        #endregion

        #region Order

        public async Task SyncOrdersOnZohoAsync(IList<int> orderIds, SyncType syncType = SyncType.DifferentialSync)
        {
            if (orderIds == null || !orderIds.Any())
                throw new ArgumentNullException(nameof(orderIds));

            var newOrders = await _zohoDataMappingService.GetZohoOrdersByOrderIdsAsync(orderIds, false);
            var existingOrders = await _zohoDataMappingService.GetZohoOrdersByOrderIdsAsync(orderIds, true);

            if (newOrders != null && newOrders.Any())
            {
                if (newOrders.Count < 100)
                    Console.WriteLine(newOrders.Count);

                var response = await SyncOrdersAsync(newOrders, Method.POST);

                var maps = new List<DataMapping>();
                var deleteIds = new List<int>();

                for (var i = 0; i < response.Data.Count; i++)
                {
                    var datum = response.Data[i];
                    if (string.IsNullOrWhiteSpace(datum.Details.Id))
                    {
                        await _logger.ErrorAsync($"Sync Error for Order, Code: {datum.Code}, Status: {datum.Status}.");
                        continue;
                    }
                    
                    maps.Add(new DataMapping
                    {
                        ZohoId = datum.Details.Id,
                        EntityTypeId = (int)EntityType.Orders,
                        EntityId = newOrders[i].Id
                    });

                    if (datum.Message == "SUCCESS")
                        deleteIds.Add(newOrders[i].Id);
                }

                await _zohoDataMappingService.InsertDataMappingAsync(maps);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Orders, deleteIds);
            }
            if (existingOrders != null && existingOrders.Any())
            {
                await SyncOrdersAsync(existingOrders, Method.PUT);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Orders, existingOrders.Select(ec => ec.Id).ToList());
            }
        }

        public async Task SyncOrdersAsync(SyncType syncType)
        {
            var pageIndex = 0;
            if (syncType == SyncType.FullSync)
            {
                while (true)
                {
                    var nopOrders = await _orderService.SearchOrdersAsync(pageIndex: pageIndex, pageSize: _pageSize);
                    if (nopOrders == null || !nopOrders.Any())
                        break;
                    var nopOrderIds = nopOrders.Select(c => c.Id).ToList();
                    await SyncOrdersOnZohoAsync(nopOrderIds, syncType);
                    pageIndex++;
                }
            }
            else if (syncType == SyncType.DifferentialSync)
            {
                while (true)
                {
                    var updatableItems = await _updatableItemService.GetAllUpdatableItemAsync(EntityType.Orders, pageIndex: pageIndex, pageSize: _pageSize);
                    if (updatableItems == null || !updatableItems.Any())
                        break;
                    var nopOrderIds = updatableItems.Select(c => c.Id).ToList();
                    //var nopOrderEntityIds = updatableItems.Select(c => c.EntityId).ToList();
                    await SyncOrdersOnZohoAsync(nopOrderIds);
                    pageIndex++;
                }

                await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Orders);
            }

            await _logger.InformationAsync($"Sync Done for Order. SyncType: {syncType}.");
        }

        #endregion

        #region Shipment

        public async Task SyncShipmentsOnZohoAsync(IList<int> shipmentIds, SyncType syncType = SyncType.DifferentialSync)
        {
            if (shipmentIds == null || !shipmentIds.Any())
                throw new ArgumentNullException(nameof(shipmentIds));

            var newShipments = await _zohoDataMappingService.GetZohoShipmentsByShipmentIdsAsync(shipmentIds, false);
            var existingShipments = await _zohoDataMappingService.GetZohoShipmentsByShipmentIdsAsync(shipmentIds, true);

            if (newShipments != null && newShipments.Any())
            {
                if (newShipments.Count < 100)
                    Console.WriteLine(newShipments.Count);

                var response = await SyncShipmentsAsync(newShipments, Method.POST);

                var maps = new List<DataMapping>();
                var deleteIds = new List<int>();

                for (var i = 0; i < response.Data.Count; i++)
                {
                    var datum = response.Data[i];
                    if (string.IsNullOrWhiteSpace(datum.Details.Id))
                    {
                        await _logger.ErrorAsync($"Sync Error for Shipment, Code: {datum.Code}, Status: {datum.Status}.");
                        continue;
                    }

                    maps.Add(new DataMapping
                    {
                        ZohoId = datum.Details.Id,
                        EntityTypeId = (int)EntityType.Shipments,
                        EntityId = newShipments[i].Id
                    });

                    if (datum.Message == "SUCCESS")
                        deleteIds.Add(newShipments[i].Id);
                }

                await _zohoDataMappingService.InsertDataMappingAsync(maps);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Shipments, deleteIds);
            }
            if (existingShipments != null && existingShipments.Any())
            {
                await SyncShipmentsAsync(existingShipments, Method.PUT);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Shipments, existingShipments.Select(ec => ec.Id).ToList());
            }
        }

        public async Task SyncShipmentsAsync(SyncType syncType)
        {
            var pageIndex = 0;
            if (syncType == SyncType.FullSync)
            {
                while (true)
                {
                    var nopShipments = await _shipmentService.GetAllShipmentsAsync(pageIndex: pageIndex, pageSize: _pageSize);
                    if (nopShipments == null || !nopShipments.Any())
                        break;
                    var nopShipmentIds = nopShipments.Select(c => c.Id).ToList();
                    await SyncShipmentsOnZohoAsync(nopShipmentIds, syncType);
                    pageIndex++;
                }
            }
            else if (syncType == SyncType.DifferentialSync)
            {
                while (true)
                {
                    var updatableItems = await _updatableItemService.GetAllUpdatableItemAsync(EntityType.Shipments, pageIndex: pageIndex, pageSize: _pageSize);
                    if (updatableItems == null || !updatableItems.Any())
                        break;
                    var nopShipmentIds = updatableItems.Select(c => c.Id).ToList();
                    await SyncShipmentsOnZohoAsync(nopShipmentIds);
                    pageIndex++;
                }

                await _updatableItemService.DeleteUpdatableItemAsync(EntityType.Shipments);
            }

            await _logger.InformationAsync($"Sync Done for Shipment. SyncType: {syncType}.");
        }

        #endregion

        #region Shipment Item

        public async Task SyncShipmentItemsOnZohoAsync(IList<int> shipmentItemIds, SyncType syncType = SyncType.DifferentialSync)
        {
            if (shipmentItemIds == null || !shipmentItemIds.Any())
                throw new ArgumentNullException(nameof(shipmentItemIds));

            var newShipmentItems = await _zohoDataMappingService.GetZohoShipmentItemsByShipmentItemIdsAsync(shipmentItemIds, false);
            var existingShipmentItems = await _zohoDataMappingService.GetZohoShipmentItemsByShipmentItemIdsAsync(shipmentItemIds, true);

            if (newShipmentItems != null && newShipmentItems.Any())
            {
                if (newShipmentItems.Count < 100)
                    Console.WriteLine(newShipmentItems.Count);

                var response = await SyncShipmentItemsAsync(newShipmentItems, Method.POST);

                var maps = new List<DataMapping>();
                var deleteIds = new List<int>();

                for (var i = 0; i < response.Data.Count; i++)
                {
                    var datum = response.Data[i];
                    if (string.IsNullOrWhiteSpace(datum.Details.Id))
                    {
                        await _logger.ErrorAsync($"Sync Error for Shipment Item, Code: {datum.Code}, Status: {datum.Status}.");
                        continue;
                    }

                    maps.Add(new DataMapping
                    {
                        ZohoId = datum.Details.Id,
                        EntityTypeId = (int)EntityType.ShipmentItems,
                        EntityId = newShipmentItems[i].Id
                    });

                    if (datum.Message == "SUCCESS")
                        deleteIds.Add(newShipmentItems[i].Id);
                }

                await _zohoDataMappingService.InsertDataMappingAsync(maps);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.ShipmentItems, deleteIds);
            }
            if (existingShipmentItems != null && existingShipmentItems.Any())
            {
                await SyncShipmentItemsAsync(existingShipmentItems, Method.PUT);

                if (syncType != SyncType.DifferentialSync)
                    await _updatableItemService.DeleteUpdatableItemAsync(EntityType.ShipmentItems, existingShipmentItems.Select(ec => ec.Id).ToList());
            }
        }

        public async Task SyncShipmentItemsAsync(SyncType syncType)
        {
            var pageIndex = 0;
            if (syncType == SyncType.FullSync)
            {
                while (true)
                {
                    var nopShipmentItems = await _zohoDataMappingService.GetAllShipmentItemsAsync(pageIndex: pageIndex, pageSize: _pageSize);
                    if (nopShipmentItems == null || !nopShipmentItems.Any())
                        break;
                    var nopShipmentItemIds = nopShipmentItems.Select(c => c.Id).ToList();
                    await SyncShipmentItemsOnZohoAsync(nopShipmentItemIds, syncType);
                    pageIndex++;
                }
            }
            else if (syncType == SyncType.DifferentialSync)
            {
                while (true)
                {
                    var updatableItems = await _updatableItemService.GetAllUpdatableItemAsync(EntityType.ShipmentItems, pageIndex: pageIndex, pageSize: _pageSize);
                    if (updatableItems == null || !updatableItems.Any())
                        break;
                    var nopShipmentItemIds = updatableItems.Select(c => c.Id).ToList();
                    await SyncShipmentItemsOnZohoAsync(nopShipmentItemIds);
                    pageIndex++;
                }

                await _updatableItemService.DeleteUpdatableItemAsync(EntityType.ShipmentItems);
            }

            await _logger.InformationAsync($"Sync Done for Shipment Item. SyncType: {syncType}.");
        }

        #endregion

        #endregion

        #endregion
    }
}
