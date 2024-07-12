using Nop.Core;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using Nop.Services.Logging;
using Nop.Services.Directory;
using Nop.Core.Domain.Shipping;
using System.Globalization;
using Nop.Services.Localization;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Shipping.DHL.Services;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Services.Catalog;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Helpers;
using Nop.Services.Security;

namespace NopStation.Plugin.Shipping.DHL
{
    public class DHLComputationMethod : BasePlugin, IShippingRateComputationMethod, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly DHLSettings _dhlSettings;
        private readonly ICountryService _countryService;
        private readonly ILogger _logger;
        private readonly StringBuilder _traceMessages;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IDHLAcceptedServicesService _dhlAcceptedServicesService;
        private readonly IDHLShipmentService _dHLShipmentService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public DHLComputationMethod(DHLSettings dhlSettings,
            ISettingService settingService,
            ILogger logger,
            ICountryService countryService,
            IWebHelper webHelper,
            IWorkContext workContext,
            ICurrencyService currencyService,
            IDHLAcceptedServicesService dhlAcceptedServicesService,
            IDHLShipmentService dHLShipmentService,
            ILocalizationService localizationService,
            IProductService productService,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService)
        {
            _dhlSettings = dhlSettings;
            _settingService = settingService;
            _logger = logger;
            _countryService = countryService;
            _traceMessages = new StringBuilder();
            _webHelper = webHelper;
            _workContext = workContext;
            _currencyService = currencyService;
            _dhlAcceptedServicesService = dhlAcceptedServicesService;
            _dHLShipmentService = dHLShipmentService;
            _localizationService = localizationService;
            _productService = productService;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
        }

        #endregion

        #region Install / Uninstall

        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new DHLSettings
            {
                TrackingUrl = "https://www.dhl.com/en/express/tracking.html"
            });

            await this.InstallPluginAsync(new DHLPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new DHLPermissionProvider());
            await base.UninstallAsync();
        }

        #endregion

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/DHL/Configure";
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.DHL.Menu.DHL")
            };

            if (await _permissionService.AuthorizeAsync(DHLPermissionProvider.ManageConfiguration))
            {
                var configure = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/DHL/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DHL.Menu.Configuration"),
                    SystemName = "DHL.Configuration"
                };
                menu.ChildNodes.Add(configure);

                var orderList = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/DHL/OrderList",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DHL.Menu.OrderList"),
                    SystemName = "DHL.OrderList"
                };
                menu.ChildNodes.Add(orderList);

                var serviceList = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/DHL/ServiceList",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DHL.Menu.ServiceList"),
                    SystemName = "DHL.ServiceList"
                };
                menu.ChildNodes.Add(serviceList);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/dhl-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=dhl",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        private async Task<(IEnumerable<ShippingOption> shippingOptions, string error)> ParseGetQouteResponseAsync(string responseXML)
        {
            var error = string.Empty;
            var shippingCharge = string.Empty;
            var service = string.Empty;
            var totalTransitDays = string.Empty;
            var globalProductCode = string.Empty;
            decimal? shippingChargeInPrimaryCurrency = null;

            var shippingOptions = new List<ShippingOption>();

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
                                            error = textReader.ReadString();
                                            return (shippingOptions, error);
                                        }
                                    }
                                }
                            }
                        }

                        if (textReader.Name == "BkgDetails" && textReader.NodeType == XmlNodeType.Element)
                        {
                            while (textReader.Read())
                            {
                                if (textReader.Name == "QtdShp" && textReader.NodeType == XmlNodeType.Element)
                                {
                                    while (textReader.Read())
                                    {
                                        if (textReader.Name == "GlobalProductCode" && textReader.NodeType == XmlNodeType.Element)
                                        {
                                            globalProductCode = textReader.ReadString();
                                        }
                                        if (textReader.Name == "ProductShortName" && textReader.NodeType == XmlNodeType.Element)
                                        {
                                            service = textReader.ReadString();
                                        }

                                        if (textReader.Name == "TotalTransitDays" && textReader.NodeType == XmlNodeType.Element)
                                        {
                                            totalTransitDays = textReader.ReadString();
                                        }

                                        if (textReader.Name == "ShippingCharge" && textReader.NodeType == XmlNodeType.Element)
                                        {
                                            shippingCharge = textReader.ReadString();

                                            var dhlCurrency = await _currencyService.GetCurrencyByIdAsync(_dhlSettings.SelectedCurrencyId);
                                            if (dhlCurrency != null)
                                            {
                                                shippingChargeInPrimaryCurrency = Convert.ToDecimal(shippingCharge, new CultureInfo("en-US"));
                                                shippingChargeInPrimaryCurrency *= _dhlSettings.SelectedCurrencyRate;
                                            }

                                            var shippingOption = new ShippingOption();
                                            var activeDHLServices = (await _dhlAcceptedServicesService.GetAllDHLServicesAsync()).Where(a => a.IsActive == true).ToList();

                                            if (activeDHLServices != null && activeDHLServices.Where(a => a.GlobalProductCode == globalProductCode).Any())
                                            {
                                                shippingOption = new ShippingOption
                                                {
                                                    Rate = shippingChargeInPrimaryCurrency.HasValue ? shippingChargeInPrimaryCurrency.Value : Convert.ToDecimal(shippingCharge, new CultureInfo("en-US")),
                                                    Name = service + " " + globalProductCode,
                                                    Description = "Total Transit Days " + totalTransitDays
                                                };
                                                shippingOptions.Add(shippingOption);
                                            }
                                            else if (activeDHLServices == null || activeDHLServices.Count == 0)
                                            {
                                                shippingOption = new ShippingOption
                                                {
                                                    Rate = shippingChargeInPrimaryCurrency.HasValue ? shippingChargeInPrimaryCurrency.Value : Convert.ToDecimal(shippingCharge, new CultureInfo("en-US")),
                                                    Name = service + " " + globalProductCode,
                                                    Description = "Total Transit Days " + totalTransitDays
                                                };
                                                shippingOptions.Add(shippingOption);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return (shippingOptions, error);
        }

        private async Task<string> CreateGetQuoteBodyAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version='1.0' encoding='UTF-8'?>");
            sb.Append("<p:DCTRequest xmlns:p='http://www.dhl.com' xmlns:p1='http://www.dhl.com/datatypes' xmlns:p2='http://www.dhl.com/DCTRequestdatatypes' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='http://www.dhl.com DCT-req.xsd '>");
            sb.Append("<GetQuote>");

            sb.Append("<Request>");
            sb.Append("<ServiceHeader>");
            sb.AppendFormat("<SiteID>{0}</SiteID>", _dhlSettings.SiteId);
            sb.AppendFormat("<Password>{0}</Password>", _dhlSettings.Password);
            sb.Append("</ServiceHeader>");
            sb.Append("</Request>");

            sb.Append("<From>");
            sb.AppendFormat("<CountryCode>{0}</CountryCode>", _dhlSettings.CountryCode);
            sb.AppendFormat("<Postalcode>{0}</Postalcode>", _dhlSettings.PostalCode);
            sb.AppendFormat("<City>{0}</City>", _dhlSettings.City);
            sb.Append("</From>");

            sb.Append(" <BkgDetails>");
            sb.AppendFormat("<PaymentCountryCode>{0}</PaymentCountryCode>", _dhlSettings.CountryCode);
            sb.AppendFormat("<Date>{0}</Date>", DateTime.Now.ToString("yyyy-MM-dd"));
            sb.AppendFormat("<ReadyTime>PT{0}H{1}M</ReadyTime>", DateTime.Now.ToString("HH"), DateTime.Now.ToString("MM"));
            sb.AppendFormat("<ReadyTimeGMTOffset>+06:00</ReadyTimeGMTOffset>");
            sb.AppendFormat("<DimensionUnit>CM</DimensionUnit>");
            sb.AppendFormat("<WeightUnit>KG</WeightUnit>");

            if (getShippingOptionRequest.Items.Any())
            {
                int count = 0;

                sb.Append("<Pieces>");

                foreach (var item in getShippingOptionRequest.Items)
                {
                    var product = await _productService.GetProductByIdAsync(item.ShoppingCartItem.ProductId)
                        ?? throw new ArgumentException("No product found with the specified id");

                    //int.TryParse(_dhlSettings.DefaultHeight.to, out height);
                    sb.Append("<Piece>");
                    sb.AppendFormat("<PieceID>{0}</PieceID>", count);
                    sb.AppendFormat("<Height>{0}</Height>", product.Height != 0 ? Math.Round(product.Height, 3).ToString() : _dhlSettings.DefaultHeight.ToString());
                    sb.AppendFormat("<Depth>{0}</Depth>", _dhlSettings.DefaultDepth.ToString());
                    sb.AppendFormat("<Width>{0}</Width>", product.Width != 0 ? Math.Round(product.Width, 3).ToString() : _dhlSettings.DefaultWidth.ToString());
                    sb.AppendFormat("<Weight>{0}</Weight>", product.Weight != 0 ? Math.Round(product.Weight, 3).ToString() : _dhlSettings.DefaultWeight.ToString());
                    sb.Append("</Piece>");
                    count++;
                }

                sb.Append("</Pieces> ");
            }

            sb.AppendFormat("<PaymentAccountNumber>{0}</PaymentAccountNumber>", _dhlSettings.AccountNumber);
            sb.Append("</BkgDetails>");

            sb.Append("<To>");
            sb.AppendFormat("<CountryCode>{0}</CountryCode>", (await _countryService.GetCountryByIdAsync((int)getShippingOptionRequest.ShippingAddress.CountryId)).TwoLetterIsoCode);
            sb.AppendFormat("<Postalcode>{0}</Postalcode>", getShippingOptionRequest.ShippingAddress.ZipPostalCode);
            sb.Append("</To>");

            //sb.Append("<Dutiable>");
            //sb.Append("<DeclaredCurrency>USD</DeclaredCurrency>");
            //sb.AppendFormat("<DeclaredValue>1.0</DeclaredValue>");
            //sb.Append("</Dutiable>");

            sb.Append("</GetQuote>");
            sb.Append("</p:DCTRequest>");

            return sb.ToString();
        }

        private string DoRequest(string url, string requestString)
        {
            var bytes = Encoding.ASCII.GetBytes(requestString);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = MimeTypes.ApplicationXWwwFormUrlencoded;
            request.ContentLength = bytes.Length;
            using (var requestStream = request.GetRequestStream())
                requestStream.Write(bytes, 0, bytes.Length);
            using (var response = request.GetResponse())
            {
                string responseXml;
                using (var reader = new StreamReader(response.GetResponseStream()))
                    responseXml = reader.ReadToEnd();

                return responseXml;
            }
        }

        #region Interface Implementation

        public async Task<GetShippingOptionResponse> GetShippingOptionsAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.ShippingAddress.CountryId == null)
            {
                response.AddError("Country is not selected");
                return response;
            }

            var country = await _countryService.GetCountryByIdAsync((int)getShippingOptionRequest.ShippingAddress.CountryId);
            if (country == null)
            {
                response.AddError("Country is not selected");
                return response;
            }

            if (getShippingOptionRequest.Items == null)
            {
                response.AddError("No shipment items");
                return response;
            }

            if (getShippingOptionRequest.ShippingAddress == null)
            {
                response.AddError("Shipping address is not set");
                return response;
            }

            if (country == null)
            {
                response.AddError("Shipping country is not set");
                return response;
            }

            if (getShippingOptionRequest.CountryFrom == null)
            {
                getShippingOptionRequest.CountryFrom = (await _countryService.GetAllCountriesAsync()).FirstOrDefault();
            }

            if (country.Name.ToLower() != "bangladesh")
            {
                try
                {
                    string requestBody = await CreateGetQuoteBodyAsync(getShippingOptionRequest);
                    await _logger.InformationAsync("DHL shipping option request body: " + requestBody);
                    var responseXML = DoRequest(_dhlSettings.Url, requestBody);
                    await _logger.InformationAsync("DHL shipping option response: " + responseXML);
                    var quoteResponse = await ParseGetQouteResponseAsync(responseXML);

                    if (string.IsNullOrEmpty(quoteResponse.error))
                    {
                        foreach (var shippingOption in quoteResponse.shippingOptions)
                        {
                            if (!shippingOption.Name.ToLower().StartsWith("dhl"))
                                shippingOption.Name = $"DHL {shippingOption.Name}";
                            response.ShippingOptions.Add(shippingOption);
                        }
                    }
                    else
                        response.AddError(quoteResponse.error);

                    if (response.ShippingOptions.Any())
                        response.Errors.Clear();
                }
                catch (Exception exc)
                {
                    response.AddError($"DHL Service is currently unavailable, try again later. {exc.Message}");
                }
                finally
                {
                    if (_dhlSettings.Tracing && _traceMessages.Length > 0)
                    {
                        var shortMessage =
                            $"DHL Get Shipping Options for customer {getShippingOptionRequest.Customer.Email}.  {getShippingOptionRequest.Items.Count} item(s) in cart";
                        await _logger.InformationAsync(shortMessage, new Exception(_traceMessages.ToString()), getShippingOptionRequest.Customer);
                    }
                }
            }

            return response;
        }

        public Task<decimal?> GetFixedRateAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            return Task.FromResult<decimal?>(null);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Menu.DHL", "DHL shipping"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Menu.OrderList", "Orders"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Menu.ServiceList", "Services"),

                new KeyValuePair<string, string>("Admin.NopStation.DHL.OrderList", "Orders"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.DhlOrders", " DHL Orders"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.ServiceList", "Service list"),

                new KeyValuePair<string, string>("Admin.NopStation.DHL.CreateDHLserviceList", "Create DHL service"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration", "DHL settings"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Url", "Url"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Url.Hint", "Enter Your Store Url"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.SiteId", "Site id"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.SiteId.Hint", "The DHL application id. Click 'Update index' if the value of this property is changed."),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Password", "Password"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Password.Hint", "Enter your password"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.AccountNumber", "Account number"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.AccountNumber.Hint", "Enter your account number"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Name.Hint", "Enter your name"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Country", "Country"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Country.Hint", "Enter your country name"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Email", "Email"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Email.Hint", "Enter your email"),

                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.TrackingUrl", "Order tracking url"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.TrackingUrl.Hint", "Enter order tracking url"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Tracing", "Tracing"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Tracing.Hint", "Mark for tracing"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.CountryCode", "Country code"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.CountryCode.Hint", "Enter your country code"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.City", "City"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.City.Hint", "Enter your city"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.PostalCode", "Postal code"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.PostalCode.Hint", "Enter your postal code"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.DefaultHeight", "Default height (in cm)"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.DefaultHeight.Hint", "Enter default height (in cm)"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.DefaultDepth", "Default depth (in cm)"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.DefaultDepth.Hint", "Enter default depth (in cm)"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.DefaultWidth", "Default width (in cm)"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.DefaultWidth.Hint", "Enter default width (in cm)"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.DefaultWeight", "Default weight (in kg)"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.DefaultWeight.Hint", "Enter default weight (in kg)"),

                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.PickupReadyByTime", "Pickup ready by time(HH:MM)"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.PickupReadyByTime.Hint", "Enter pickup ready by time(HH:MM)"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.PickupCloseTime", "Pickup close time(HH:MM)"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.PickupCloseTime.Hint", "Enter pickup close time(HH:MM)"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.PickupPackageLocation", "Pickup package location"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.PickupPackageLocation.Hint", "Enter pickup package location"),

                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Currency", "Currency"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.Currency.Hint", "Enter currency name."),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.CurrencyRate", "Currency rate"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.CurrencyRate.Hint", "Enter currency rate."),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Currency.Fields.Active", "Active"),

                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.CompanyName", "Company name"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.CompanyName.Hint", "Enter your company name."),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.CompanyAddress", "Company address"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.CompanyAddress.Hint", "Enter your company address."),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.PhoneNumber", "Phone number"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Configuration.Fields.PhoneNumber.Hint", "Enter your Phone number."),

                new KeyValuePair<string, string>("Admin.NopStation.DHL.Services.BackToList", "back to service list"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Services.Fields.ServiceName", "Service name"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Services.Fields.GlobalProductCode", "Global product code"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Services.Fields.IsActive", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Alert.SelectedCurrencyRate.Failed", "Failed to retrieve currency rate."),

                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.List", "Orders"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.CustomOrderNumber", "Custom order number"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.StoreName", "Store name"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.CustomerEmail", "Customer email"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.CustomerFullName", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.OrderTotal", "Order total"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.OrderStatus", "Order status"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.PaymentStatus", "Payment status"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.ShippingStatus", "Shipping status"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.HasShippingLabel", "Has shipping label"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.CanBookPickup", "Can book pickup"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.AirwayBillNumber", "Airway bill number"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.ConfirmationNumber", "Confirmation number"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.ReadyByTime", "Ready by time"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.NextPickupDate", "Next pickup date"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.AirwayBillNumber", "Airway Bill Number"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.ConfirmationNumber", "Confirmation Number"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.ReadyByTime", "Ready By Time"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Orders.Fields.ShippingLabel", "Shipping Label"),

                new KeyValuePair<string, string>("Admin.NopStation.DHL.Services.Fields.ServiceName.Required", "Service Name is required."),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Services.Fields.GlobalProductCode.Required", "Global Product Code is required."),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Services.Fields.IsActive.Required", "Active is required."),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Services.AddNew", "Add new Service"),
                new KeyValuePair<string, string>("Admin.NopStation.DHL.Services.List", "Services"),
            };

            return list;
        }

        public Task<IShipmentTracker> GetShipmentTrackerAsync()
        {
            return Task.FromResult<IShipmentTracker>(new DHLShipmentTracker(_logger, _dHLShipmentService, _dhlSettings));
        }

        #endregion
    }
}
