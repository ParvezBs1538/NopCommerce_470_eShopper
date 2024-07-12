using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Shipping.DHL.Domain;
using NopStation.Plugin.Shipping.DHL.Areas.Admin.Models;
using NopStation.Plugin.Shipping.DHL.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Nop.Services.Logging;
using Nop.Core.Domain.Shipping;
using Nop.Services.Shipping;
using Nop.Services.Messages;
using Nop.Core.Domain.Directory;
using Nop.Services.Common;
using NopStation.Plugin.Misc.Core.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Factories;
using NopStation.Plugin.Shipping.DHL.Areas.Admin.Factories;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Shipping.DHL.Areas.Admin.Controllers
{
    public class DHLController : NopStationAdminController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly CurrencySettings _currencySettings;
        private readonly DHLSettings _dhlSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IAddressService _addressService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;
        private readonly ICountryService _countryService;
        private readonly IDHLAcceptedServicesService _dhlAcceptedServicesService;
        private readonly IDHLShipmentService _dhlShipmentService;
        private readonly IDHLPickupRequestService _dHLPickupRequestService;
        private readonly ILogger _logger;
        private readonly IShipmentService _shipmentService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly INotificationService _notificationService;
        private readonly IStoreContext _storeContext;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDHLModelFactory _dhlModelFactory;

        #endregion

        #region Ctor
        public DHLController(ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            CurrencySettings currencySettings,
            DHLSettings dhlSettings,
            ICurrencyService currencyService,
            IWorkContext workContext,
            IPriceCalculationService priceCalculationService,
            IAddressService addressService,
            IOrderService orderService,
            IProductService productService,
            IStoreService storeService,
            ICountryService countryService,
            IDHLAcceptedServicesService dhlAcceptedServicesService,
            IDHLShipmentService dhlShipmentService,
            IDHLPickupRequestService dHLPickupRequestService,
            ILogger logger,
            IShipmentService shipmentService,
            ICustomerActivityService customerActivityService,
            INotificationService notificationService,
            IStoreContext storeContext,
            IBaseAdminModelFactory baseAdminModelFactory,
            IDHLModelFactory dhlModelFactory)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _currencySettings = currencySettings;
            _dhlSettings = dhlSettings;
            _currencyService = currencyService;
            _workContext = workContext;
            _priceCalculationService = priceCalculationService;
            _addressService = addressService;
            _orderService = orderService;
            _productService = productService;
            _storeService = storeService;
            _countryService = countryService;
            _dhlAcceptedServicesService = dhlAcceptedServicesService;
            _dhlShipmentService = dhlShipmentService;
            _dHLPickupRequestService = dHLPickupRequestService;
            _logger = logger;
            _shipmentService = shipmentService;
            _customerActivityService = customerActivityService;
            _notificationService = notificationService;
            _storeContext = storeContext;
            _baseAdminModelFactory = baseAdminModelFactory;
            _dhlModelFactory = dhlModelFactory;
        }

        #endregion

        #region Utilities

        private async Task CreateShipmentAsync(int orderId, string trackingNumber)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            //get order items
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: (await _workContext.GetCurrentVendorAsync())?.Id ?? 0);

            Shipment shipment = null;
            decimal? totalWeight = null;
            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId)
                    ?? throw new ArgumentException("No product found with the specified id", nameof(orderItem.ProductId));

                //is shippable
                if (!product.IsShipEnabled)
                    continue;

                //ensure that this product can be shipped (have at least one item to ship)
                var maxQtyToAdd = await _orderService.GetTotalNumberOfItemsCanBeAddedToShipmentAsync(orderItem);
                if (maxQtyToAdd <= 0)
                    continue;

                var qtyToAdd = orderItems.Count; //parse quantity

                var warehouseId = 0;

                warehouseId = product.WarehouseId;

                var orderItemTotalWeight = orderItem.ItemWeight.HasValue ? orderItem.ItemWeight * qtyToAdd : null;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }
                if (shipment == null)
                {
                    shipment = new Shipment
                    {
                        OrderId = order.Id,
                        TrackingNumber = trackingNumber,
                        TotalWeight = null,
                        ShippedDateUtc = null,
                        DeliveryDateUtc = null,
                        AdminComment = null,
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                }

                //create a shipment item
                var shipmentItem = new ShipmentItem
                {
                    OrderItemId = orderItem.Id,
                    Quantity = qtyToAdd,
                    WarehouseId = warehouseId
                };

                shipment.TotalWeight = totalWeight;
                await _shipmentService.InsertShipmentAsync(shipment);

                shipmentItem.ShipmentId = shipment.Id;
                await _shipmentService.InsertShipmentItemAsync(shipmentItem);

                //add a note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = "A shipment has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                await _orderService.UpdateOrderAsync(order);
                await LogEditOrderAsync(order.Id);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Orders.Shipments.Added"));
            }

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.Shipments.NoProductsSelected"));
        }

        protected virtual async Task LogEditOrderAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            await _customerActivityService.InsertActivityAsync("EditOrder", await _localizationService.GetResourceAsync("ActivityLog.EditOrder"), order);
        }

        protected DHLShipmentValidationResponseModel ParseShipmentValidationResponse(string responseXML)
        {
            var messageReference = string.Empty;
            var airwayBillNumber = string.Empty;
            var base64Pdf = string.Empty;

            var responseModel = new DHLShipmentValidationResponseModel();

            using (var stringReader = new StringReader(responseXML))
            {
                using (var textReader = new XmlTextReader(stringReader))
                {
                    while (textReader.Read())
                    {
                        if (textReader.Name == "ActionStatus" && textReader.NodeType == XmlNodeType.Element)
                        {
                            while (textReader.Read())
                            {
                                if (textReader.Name == "Condition" && textReader.NodeType == XmlNodeType.Element)
                                {
                                    while (textReader.Read())
                                    {
                                        if (textReader.Name == "ConditionData" && textReader.NodeType == XmlNodeType.Element)
                                        {
                                            responseModel.Error = textReader.ReadString();
                                            return responseModel;
                                        }
                                    }
                                }
                            }
                        }

                        if (textReader.Name == "MessageReference" && textReader.NodeType == XmlNodeType.Element)
                        {
                            while (textReader.Read())
                            {
                                messageReference = textReader.ReadString();
                                break;
                            }
                        }

                        if (textReader.Name == "AirwayBillNumber" && textReader.NodeType == XmlNodeType.Element)
                        {
                            while (textReader.Read())
                            {
                                airwayBillNumber = textReader.ReadString();
                                break;
                            }
                        }

                        if (textReader.Name == "LabelImage" && textReader.NodeType == XmlNodeType.Element)
                        {
                            while (textReader.Read())
                            {
                                if (textReader.Name == "OutputFormat" && textReader.NodeType == XmlNodeType.Element)
                                {
                                    if (textReader.ReadString() == "PDF")
                                    {
                                        while (textReader.Read())
                                        {
                                            if (textReader.Name == "OutputImage" && textReader.NodeType == XmlNodeType.Element)
                                            {
                                                base64Pdf = textReader.ReadString();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            responseModel.AirwayBillNumber = airwayBillNumber;
            responseModel.MessageReference = messageReference;
            responseModel.ShippingLabelBase64Pdf = base64Pdf;

            return responseModel;
        }

        protected DHLBookPickupResponseModel ParseBookPickupResponse(string responseXML)
        {
            var messageReference = string.Empty;
            var confirmationNumber = string.Empty;
            var readyByTime = (DateTime?)null;
            var nextPickupDate = (DateTime?)null;

            var responseModel = new DHLBookPickupResponseModel();

            using (var stringReader = new StringReader(responseXML))
            {
                using (var textReader = new XmlTextReader(stringReader))
                {
                    while (textReader.Read())
                    {
                        if (textReader.Name == "ActionStatus" && textReader.NodeType == XmlNodeType.Element)
                        {
                            while (textReader.Read())
                            {
                                if (textReader.Name == "Condition" && textReader.NodeType == XmlNodeType.Element)
                                {
                                    while (textReader.Read())
                                    {
                                        if (textReader.Name == "ConditionData" && textReader.NodeType == XmlNodeType.Element)
                                        {
                                            responseModel.Error = textReader.ReadString();
                                            return responseModel;
                                        }
                                    }
                                }
                            }
                        }

                        if (textReader.Name == "MessageReference" && textReader.NodeType == XmlNodeType.Element)
                        {
                            while (textReader.Read())
                            {
                                messageReference = textReader.ReadString();
                                break;
                            }
                        }

                        if (textReader.Name == "ConfirmationNumber" && textReader.NodeType == XmlNodeType.Element)
                        {
                            while (textReader.Read())
                            {
                                confirmationNumber = textReader.ReadString();
                                break;
                            }
                        }

                        if (textReader.Name == "ReadyByTime" && textReader.NodeType == XmlNodeType.Element)
                        {
                            while (textReader.Read())
                            {
                                if (DateTime.TryParse(textReader.ReadString(), out var dt))
                                    readyByTime = dt;
                                break;
                            }
                        }

                        if (textReader.Name == "NextPickupDate" && textReader.NodeType == XmlNodeType.Element)
                        {
                            while (textReader.Read())
                            {
                                if (DateTime.TryParse(textReader.ReadString(), out var dt))
                                    nextPickupDate = dt;
                                break;
                            }
                        }
                    }
                }
            }

            responseModel.MessageReference = messageReference;
            responseModel.ConfirmationNumber = confirmationNumber;
            responseModel.ReadyByTime = readyByTime;
            responseModel.NextPickupDate = nextPickupDate;

            return responseModel;
        }

        protected IEnumerable<string> GetAddress(string str, int iterateCount)
        {
            var words = new List<string>();

            for (var i = 0; i < str.Length; i += iterateCount)
                if (str.Length - i >= iterateCount)
                    words.Add(str.Substring(i, iterateCount));
                else
                    words.Add(str.Substring(i, str.Length - i));

            return words;
        }

        protected async Task<string> DoRequest(string url, string requestBody)
        {
            var bytes = Encoding.ASCII.GetBytes(requestBody);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = MimeTypes.ApplicationXWwwFormUrlencoded;
            request.ContentLength = bytes.Length;
            using (var requestStream = request.GetRequestStream())
                await requestStream.WriteAsync(bytes, 0, bytes.Length);
            using (var response = await request.GetResponseAsync())
            {
                string responseXml;
                using (var reader = new StreamReader(response.GetResponseStream()))
                    responseXml = reader.ReadToEnd();

                return responseXml;
            }
        }

        protected async Task<string> CreateShipmentValidationBodyAsync(Order order)
        {
            var shippingAddress = await _addressService.GetAddressByIdAsync((int)order.ShippingAddressId)
                       ?? throw new ArgumentException("No address found with the specified id", nameof(order.ShippingAddressId));

            //get order items
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: (await _workContext.GetCurrentVendorAsync())?.Id ?? 0);

            var sb = new StringBuilder();
            var messageReference = new StringBuilder();

            var tempRef = order.Id + "-" + DateTime.Now.ToString("yyyy-MM-dd");
            messageReference.Append(tempRef.PadLeft(32, '0'));

            sb.Append("<?xml version='1.0' encoding='UTF-8'?>");
            sb.Append("<req:ShipmentRequest xmlns:req='http://www.dhl.com' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='http://www.dhl.com ship-val-global-req.xsd' schemaVersion='5.0'>");

            sb.Append("<Request>");
            sb.Append("<ServiceHeader>");
            sb.AppendFormat("<MessageTime>{0}</MessageTime>", DateTime.Now.ToString("yyyy-MM-ddTHH:MM:ss"));
            sb.AppendFormat("<MessageReference>{0}</MessageReference>", messageReference.ToString());
            sb.AppendFormat("<SiteID>{0}</SiteID>", _dhlSettings.SiteId);
            sb.AppendFormat("<Password>{0}</Password>", _dhlSettings.Password);
            sb.Append("</ServiceHeader>");
            sb.Append("</Request>");
            sb.Append("<LanguageCode>en</LanguageCode>");
            sb.Append("<PiecesEnabled>Y</PiecesEnabled>");

            sb.Append("<Billing>");
            sb.AppendFormat("<ShipperAccountNumber>{0}</ShipperAccountNumber>", _dhlSettings.AccountNumber);
            sb.Append("<ShippingPaymentType>S</ShippingPaymentType>");
            sb.Append("<DutyPaymentType>S</DutyPaymentType>");
            sb.Append("</Billing>");

            sb.Append("<Consignee>");
            sb.Append("<CompanyName>Personal</CompanyName>");

            if (shippingAddress.Address1.Count() > 32)
            {
                var address1 = GetAddress(shippingAddress.Address1, 32);

                foreach (var address in address1)
                {
                    sb.Append("<AddressLine>");
                    sb.Append(address);
                    sb.Append("</AddressLine>");
                }
            }
            else
            {
                sb.Append("<AddressLine>");
                sb.Append(shippingAddress.Address1);
                sb.Append("</AddressLine>");
            }

            if (!string.IsNullOrEmpty(shippingAddress.Address2))
            {
                if (shippingAddress.Address2.Count() > 32)
                {
                    var address2 = GetAddress(shippingAddress.Address2, 32);

                    foreach (var address in address2)
                    {
                        sb.Append("<AddressLine>");
                        sb.Append(address);
                        sb.Append("</AddressLine>");
                    }

                }
                else
                {
                    sb.Append("<AddressLine>");
                    sb.Append(shippingAddress.Address2);
                    sb.Append("</AddressLine>");
                }
            }

            var country = await _countryService.GetCountryByIdAsync((int)shippingAddress.CountryId)
                       ?? throw new ArgumentException("No country found with the specified id", nameof(shippingAddress.CountryId));

            sb.AppendFormat("<City>{0}</City>", shippingAddress.City);
            sb.AppendFormat("<PostalCode>{0}</PostalCode>", shippingAddress.ZipPostalCode);
            sb.AppendFormat("<CountryCode>{0}</CountryCode>", country.TwoLetterIsoCode);
            sb.AppendFormat("<CountryName>{0}</CountryName>", country.Name);
            sb.Append("<Contact>");
            sb.AppendFormat("<PersonName>{0}</PersonName>", shippingAddress.FirstName + " " + shippingAddress.LastName);
            sb.AppendFormat("<PhoneNumber>{0}</PhoneNumber>", shippingAddress.PhoneNumber);
            sb.AppendFormat("<Email>{0}</Email>", shippingAddress.Email);
            sb.Append("</Contact>");
            sb.Append("</Consignee>");

            sb.Append("<Reference>");
            sb.AppendFormat("<ReferenceID>{0}</ReferenceID>", _dhlSettings.AccountNumber.ToString());
            sb.Append("</Reference>");

            sb.Append("<ShipmentDetails>");
            sb.AppendFormat("<NumberOfPieces>{0}</NumberOfPieces>", orderItems.Count.ToString());

            if (orderItems.Any())
            {
                int count = 1;

                sb.Append("<Pieces>");

                foreach (var item in orderItems)
                {
                    sb.Append("<Piece>");
                    sb.AppendFormat("<PieceID>{0}</PieceID>", count);
                    sb.Append("</Piece>");
                    count++;
                }

                sb.Append("</Pieces>");
            }

            sb.AppendFormat("<Weight>{0}</Weight>", orderItems.Sum(item => item.ItemWeight) != 0 ? Math.Round(orderItems.Sum(item => item.ItemWeight ?? 0), 3) : _dhlSettings.DefaultWeight * orderItems.Count);
            sb.Append("<WeightUnit>K</WeightUnit>");

            var shipmentService = _dhlShipmentService.GetDHLShipmentSubmissionByOrderId(order.Id);

            if (shipmentService != null)
            {
                sb.AppendFormat("<GlobalProductCode>{0}</GlobalProductCode>", shipmentService.GlobalProductCode);
            }

            sb.AppendFormat("<Date>{0}</Date>", DateTime.Now.ToString("yyyy-MM-dd"));
            sb.Append("<Contents>DHL shipment through nopcommerce site</Contents>");
            sb.Append("<DimensionUnit>C</DimensionUnit>");
            sb.Append("<CurrencyCode>USD</CurrencyCode>");
            sb.Append("</ShipmentDetails>");

            sb.Append("<Shipper>");
            sb.AppendFormat("<ShipperID>{0}</ShipperID>", _dhlSettings.AccountNumber);
            sb.AppendFormat("<CompanyName>{0}</CompanyName>", _dhlSettings.CompanyName);
            var companyAddress = GetAddress(_dhlSettings.CompanyAddress, 40);

            foreach (var address in companyAddress)
            {
                sb.Append("<AddressLine>");
                sb.Append(address);
                sb.Append("</AddressLine>");
            }

            sb.AppendFormat("<City>{0}</City>", _dhlSettings.City);
            sb.AppendFormat("<CountryCode>{0}</CountryCode>", _dhlSettings.CountryCode);
            sb.AppendFormat("<CountryName>{0}</CountryName>", _dhlSettings.Country);
            sb.Append("<Contact>");
            sb.AppendFormat("<PersonName>{0}</PersonName>", _dhlSettings.Name);
            sb.AppendFormat("<PhoneNumber>{0}</PhoneNumber>", _dhlSettings.PhoneNumber);
            sb.AppendFormat("<Email>{0}</Email>", _dhlSettings.Email);
            sb.Append("</Contact>");
            sb.Append("</Shipper>");
            sb.Append("<LabelImageFormat>PDF</LabelImageFormat>");
            sb.Append("</req:ShipmentRequest>");
            return sb.ToString();
        }

        protected async Task<string> CreatePickupRequestBodyAsync(Order order, string awbNumber)
        {
            //get order items
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: (await _workContext.GetCurrentVendorAsync())?.Id ?? 0);

            var sb = new StringBuilder();
            var messageReference = new StringBuilder();

            var tempRef = order.Id + "-" + DateTime.Now.ToString("yyyy-MM-dd");
            messageReference.Append(tempRef.PadLeft(32, '0'));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId);

            sb.Append("<?xml version='1.0' encoding='UTF-8'?>");
            sb.Append("<req:BookPURequest xmlns:req='http://www.dhl.com' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='http://www.dhl.com pickupdatatypes_global-3.0.xsd' schemaVersion='3.0'>");

            sb.Append("<Request>");
            sb.Append("<ServiceHeader>");
            sb.AppendFormat("<MessageTime>{0}</MessageTime>", DateTime.Now.ToString("yyyy-MM-ddTHH:MM:ss"));
            sb.AppendFormat("<MessageReference>{0}</MessageReference>", messageReference.ToString());
            sb.AppendFormat("<SiteID>{0}</SiteID>", _dhlSettings.SiteId);
            sb.AppendFormat("<Password>{0}</Password>", _dhlSettings.Password);
            sb.Append("</ServiceHeader>");
            sb.Append("<MetaData>");
            sb.AppendFormat("<SoftwareName>{0}</SoftwareName>", store?.Name);
            sb.AppendFormat("<SoftwareVersion>1.0</SoftwareVersion>");
            sb.Append("</MetaData>");
            sb.Append("</Request>");

            sb.Append("<Requestor>");
            sb.Append("<AccountType>D</AccountType>");
            sb.AppendFormat("<AccountNumber>{0}</AccountNumber>", _dhlSettings.AccountNumber);
            sb.Append("<RequestorContact>");
            sb.AppendFormat("<PersonName>{0}</PersonName>", _dhlSettings.Name);
            sb.AppendFormat("<Phone>{0}</Phone>", _dhlSettings.PhoneNumber);
            sb.Append("</RequestorContact>");
            sb.AppendFormat("<CompanyName>{0}</CompanyName>", _dhlSettings.CompanyName);
            if (_dhlSettings.CompanyAddress.Length > 32)
            {
                var address1 = GetAddress(_dhlSettings.CompanyAddress, 32);

                foreach (var address in address1)
                {
                    sb.Append("<Address1>");
                    sb.Append(address);
                    sb.Append("</Address1>");
                }
            }
            else
            {
                sb.Append("<Address1>");
                sb.Append(_dhlSettings.CompanyAddress);
                sb.Append("</Address1>");
            }
            sb.AppendFormat("<City>{0}</City>", _dhlSettings.City);
            sb.AppendFormat("<CountryCode>{0}</CountryCode>", _dhlSettings.CountryCode);
            sb.AppendFormat("<PostalCode>{0}</PostalCode>", _dhlSettings.PostalCode);
            sb.Append("</Requestor>");

            sb.Append("<Place>");
            sb.AppendFormat("<LocationType>B</LocationType>");
            sb.AppendFormat("<CompanyName>{0}</CompanyName>", _dhlSettings.CompanyName);
            if (_dhlSettings.CompanyAddress.Length > 32)
            {
                var address1 = GetAddress(_dhlSettings.CompanyAddress, 32);

                foreach (var address in address1)
                {
                    sb.Append("<Address1>");
                    sb.Append(address);
                    sb.Append("</Address1>");
                }
            }
            else
            {
                sb.Append("<Address1>");
                sb.Append(_dhlSettings.CompanyAddress);
                sb.Append("</Address1>");
            }
            sb.AppendFormat("<PackageLocation>{0}</PackageLocation>", _dhlSettings.PickupPackageLocation);
            sb.AppendFormat("<City>{0}</City>", _dhlSettings.City);
            sb.AppendFormat("<CountryCode>{0}</CountryCode>", _dhlSettings.CountryCode);
            sb.Append("</Place>");

            sb.Append("<Pickup>");
            sb.AppendFormat("<PickupDate>{0}</PickupDate>", DateTime.Now.ToString("yyyy-MM-dd"));
            sb.AppendFormat("<PickupTypeCode>S</PickupTypeCode>");
            sb.AppendFormat("<ReadyByTime>{0}</ReadyByTime>", _dhlSettings.PickupReadyByTime);
            sb.AppendFormat("<CloseTime>{0}</CloseTime>", _dhlSettings.PickupCloseTime);
            sb.Append("</Pickup>");

            sb.Append("<PickupContact>");
            sb.AppendFormat("<PersonName>{0}</PersonName>", _dhlSettings.Name);
            sb.AppendFormat("<Phone>{0}</Phone>", _dhlSettings.PhoneNumber);
            sb.Append("</PickupContact>");

            sb.Append("<ShipmentDetails>");
            sb.AppendFormat("<AccountType>D</AccountType>");
            sb.AppendFormat("<AccountNumber>{0}</AccountNumber>", _dhlSettings.AccountNumber);
            sb.AppendFormat("<BillToAccountNumber>{0}</BillToAccountNumber>", _dhlSettings.AccountNumber);
            sb.AppendFormat("<AWBNumber>{0}</AWBNumber>", awbNumber);
            sb.AppendFormat("<NumberOfPieces>{0}</NumberOfPieces>", orderItems.Count.ToString());
            sb.AppendFormat("<Weight>{0}</Weight>", orderItems.Sum(item => item.ItemWeight) != 0 ? Math.Round(orderItems.Sum(item => (decimal)item.ItemWeight), 3) : _dhlSettings.DefaultWeight * orderItems.Count);
            sb.AppendFormat("<WeightUnit>K</WeightUnit>");
            sb.AppendFormat("<DoorTo>DD</DoorTo>");
            sb.Append("</ShipmentDetails>");

            sb.Append("</req:BookPURequest>");
            return sb.ToString();
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = (await _storeContext.GetActiveStoreScopeConfigurationAsync());
            var settings = await _settingService.LoadSettingAsync<DHLSettings>(storeScope);

            var model = settings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeScope;

            await _baseAdminModelFactory.PrepareCurrenciesAsync(model.AvailableCurrencies);

            if (storeScope == 0)
                return View(model);

            model.Url_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Url, storeScope);
            model.SiteId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.SiteId, storeScope);
            model.Password_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Password, storeScope);
            model.AccountNumber_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AccountNumber, storeScope);
            model.Email_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Email, storeScope);
            model.Name_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Name, storeScope);
            model.Country_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Country, storeScope);
            model.CountryCode_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.CountryCode, storeScope);
            model.PostalCode_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PostalCode, storeScope);
            model.City_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.City, storeScope);
            model.PhoneNumber_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PhoneNumber, storeScope);
            model.DefaultHeight_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DefaultHeight, storeScope);
            model.DefaultDepth_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DefaultDepth, storeScope);
            model.DefaultWidth_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DefaultWidth, storeScope);
            model.DefaultWeight_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DefaultWeight, storeScope);
            model.PickupReadyByTime_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PickupReadyByTime, storeScope);
            model.PickupCloseTime_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PickupCloseTime, storeScope);
            model.PickupPackageLocation_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PickupPackageLocation, storeScope);
            model.Tracing_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Tracing, storeScope);
            model.CompanyName_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.CompanyName, storeScope);
            model.CompanyAddress_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.CompanyAddress, storeScope);
            model.TrackingUrl_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TrackingUrl, storeScope);
            model.SelectedCurrencyId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.SelectedCurrencyId, storeScope);
            model.SelectedCurrencyRate_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.SelectedCurrencyRate, storeScope);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<DHLSettings>(storeScope);
            settings = model.ToSettings(settings);

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Url, model.Url_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SiteId, model.SiteId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AccountNumber, model.AccountNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Email, model.Email_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Name, model.Name_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Country, model.Country_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CountryCode, model.CountryCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PostalCode, model.PostalCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.City, model.City_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PhoneNumber, model.PhoneNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultHeight, model.DefaultHeight_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultDepth, model.DefaultDepth_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultWidth, model.DefaultWidth_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultWeight, model.DefaultWeight_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PickupReadyByTime, model.PickupReadyByTime_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PickupCloseTime, model.PickupCloseTime_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PickupPackageLocation, model.PickupPackageLocation_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Tracing, model.Tracing_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CompanyName, model.CompanyName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CompanyAddress, model.CompanyAddress_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TrackingUrl, model.TrackingUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SelectedCurrencyId, model.SelectedCurrencyId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SelectedCurrencyRate, model.SelectedCurrencyRate_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        public virtual async Task<IActionResult> SelectedCurrencyRate(int currencyId)
        {
            if (!await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var primaryExchangeRateCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryExchangeRateCurrencyId);
            if (primaryExchangeRateCurrency == null)
                throw new Exception("Primary exchange rate currency cannot be loaded");

            var selectedCurrency = await _currencyService.GetCurrencyByIdAsync(currencyId)
                ?? throw new ArgumentException("No currency found with the specified id");

            var selectedCurrencyRate = await _currencyService.ConvertToPrimaryExchangeRateCurrencyAsync(primaryExchangeRateCurrency.Rate, selectedCurrency);
            selectedCurrencyRate = _priceCalculationService.Round(selectedCurrencyRate, selectedCurrency.RoundingType);

            return Json(selectedCurrencyRate);
        }

        public IActionResult ServiceList()
        {
            var searchModel = new DHLServiceSearchModel();
            return View(searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> ServiceList(DHLServiceSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            //prepare grid model
            var model = await _dhlModelFactory.PrepareDHLServiceListModelAsync(searchModel);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> UpdateDHLService(DHLServiceModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var dHLService = await _dhlAcceptedServicesService.GetDHLServiceByIdAsync(model.Id);
                dHLService.GlobalProductCode = model.GlobalProductCode;
                dHLService.Name = model.ServiceName;
                dHLService.IsActive = model.IsActive;

                await _dhlAcceptedServicesService.UpdateDHLServiceAsync(dHLService);
            }

            return new NullJsonResult();
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = new DHLServiceModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DHLServiceModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (model != null)
            {
                var serviceModel = new DHLService
                {
                    GlobalProductCode = model.GlobalProductCode,
                    IsActive = model.IsActive,
                    Name = model.ServiceName
                };

                await _dhlAcceptedServicesService.InsertDHLServiceAsync(serviceModel);
            }

            return RedirectToAction("ServiceList");
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<IActionResult> DeleteDHLService(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //try to get a product review with the specified id
            var dhlService = await _dhlAcceptedServicesService.GetDHLServiceByIdAsync(id);
            if (dhlService == null)
                throw new ArgumentException("No dhl service found with the specified id");

            await _dhlAcceptedServicesService.DeleteDHLServiceAsync(dhlService);

            return new NullJsonResult();
        }

        public async Task<IActionResult> OrderList()
        {
            if (!await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            return View(new DHLOrderSearchModel());
        }

        [HttpPost]
        public async Task<IActionResult> OrderList(DHLOrderSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            //prepare grid model
            var model = await _dhlModelFactory.PrepareDHLOrderListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendDHLShipmentRequest(int orderId)
        {
            if (!await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var order = await _orderService.GetOrderByIdAsync(orderId)
                ?? throw new ArgumentNullException("No order found with the specific id.");

            var dhlshipment = _dhlShipmentService.GetDHLShipmentSubmissionByOrderId(order.Id)
                ?? throw new ArgumentException("No dhl shipment found with the specified order.");

            if (dhlshipment != null && dhlshipment.AirwayBillNumber != null && dhlshipment.MessageReference != null)
            {
                return Json(
                  new
                  {
                      success = true,
                      alreadySubmitted = true,
                      airwayBillNumber = dhlshipment.AirwayBillNumber,
                      messageReference = dhlshipment.MessageReference
                  });
            }

            var requestBody = await CreateShipmentValidationBodyAsync(order);
            var responseXML = await DoRequest(_dhlSettings.Url, requestBody);
            var responseModel = ParseShipmentValidationResponse(responseXML);

            if (string.IsNullOrEmpty(responseModel.Error))
            {
                var model = _dhlShipmentService.GetDHLShipmentSubmissionByOrderId(order.Id);
                model.AirwayBillNumber = responseModel.AirwayBillNumber;
                model.MessageReference = responseModel.MessageReference;
                model.ShippingLabelBase64Pdf = responseModel.ShippingLabelBase64Pdf;
                await _dhlShipmentService.UpdateShipmentSubmission(model);

                await CreateShipmentAsync(order.Id, responseModel.AirwayBillNumber);

                return Json(
                   new
                   {
                       success = true,
                       airwayBillNumber = responseModel.AirwayBillNumber,
                       messageReference = responseModel.MessageReference
                   });
            }

            await _logger.ErrorAsync("DHL: " + responseModel.Error);
            return Json(
                new
                {
                    success = false,
                    message = responseModel.Error
                });
        }

        [HttpPost]
        public async Task<IActionResult> BookPickupRequest(int orderId)
        {
            if (!await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var order = await _orderService.GetOrderByIdAsync(orderId)
                ?? throw new ArgumentNullException("No order found with the specific id.");

            var dhlshipment = _dhlShipmentService.GetDHLShipmentSubmissionByOrderId(order.Id)
                ?? throw new ArgumentException("No dhl shipment found with the specified order.");

            if (string.IsNullOrEmpty(dhlshipment.AirwayBillNumber))
                throw new ArgumentException("No dhl airway bill number found with the specified order.");

            var alreadyBookedPickupRequest = _dHLPickupRequestService.GetDHLPickupRequestByOrderId(order.Id);

            if (alreadyBookedPickupRequest != null)
            {
                return Json(
                  new
                  {
                      success = true,
                      alreadyBooked = true,
                      confirmationNumber = alreadyBookedPickupRequest.ConfirmationNumber,
                      readyByTime = alreadyBookedPickupRequest.ReadyByTime,
                      nextPickupDate = alreadyBookedPickupRequest.NextPickupDate
                  });
            }

            var requestBody = await CreatePickupRequestBodyAsync(order, dhlshipment.AirwayBillNumber);
            var responseXML = await DoRequest(_dhlSettings.Url, requestBody);

            var responseModel = ParseBookPickupResponse(responseXML);

            if (string.IsNullOrEmpty(responseModel.Error))
            {
                var dhlPickupRequest = new DHLPickupRequest
                {
                    OrderId = order.Id,
                    MessageReference = responseModel.MessageReference,
                    ConfirmationNumber = responseModel.ConfirmationNumber,
                    ReadyByTime = responseModel.ReadyByTime,
                    NextPickupDate = responseModel.NextPickupDate
                };

                await _dHLPickupRequestService.InsertDHLPickupRequestAsync(dhlPickupRequest);

                return Json(
                   new
                   {
                       success = true,
                       messageReference = responseModel.MessageReference,
                       confirmationNumber = responseModel.ConfirmationNumber,
                       readyByTime = responseModel.ReadyByTime,
                       nextPickupDate = responseModel.NextPickupDate
                   });
            }

            await _logger.ErrorAsync("DHL pickup request error: " + responseModel.Error);
            return Json(new
            {
                success = false,
                message = responseModel.Error
            });
        }

        public virtual async Task<IActionResult> GenerateShippingLabel(int orderId)
        {
            if (!await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var order = await _orderService.GetOrderByIdAsync(orderId)
                ?? throw new ArgumentNullException("No order found with the specific id.");

            var dhlshipment = _dhlShipmentService.GetDHLShipmentSubmissionByOrderId(order.Id)
                ?? throw new ArgumentException("No dhl shipment found with the specified order.");

            if (string.IsNullOrEmpty(dhlshipment.ShippingLabelBase64Pdf))
                throw new ArgumentException("No dhl shipping label found with the specified order.");

            var bytes = Convert.FromBase64String(dhlshipment.ShippingLabelBase64Pdf);

            return File(bytes, MimeTypes.ApplicationPdf, $"dhl_shipping_label_order_{order.Id}.pdf");
        }

        #endregion
    }
}
