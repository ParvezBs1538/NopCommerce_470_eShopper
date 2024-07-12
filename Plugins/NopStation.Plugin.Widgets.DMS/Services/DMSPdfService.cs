using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Common.Pdf;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Vendors;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Services.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public class DMSPdfService : IDMSPdfService
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IAttributeFormatter<AddressAttribute, AddressAttributeValue> _addressAttributeFormatter;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IMeasureService _measureService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IQrCodeService _qrCodeService;
        private readonly ISettingService _settingService;
        private readonly IShipmentService _shipmentService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly DMSSettings _dMSSettings;
        private readonly MeasureSettings _measureSettings;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Ctor

        public DMSPdfService(AddressSettings addressSettings,
            CatalogSettings catalogSettings,
            IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
            IAddressService addressService,
            ICountryService countryService,
            ICurrencyService currencyService,
            IHtmlFormatter htmlFormatter,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            IOrderService orderService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IQrCodeService qrCodeService,
            ISettingService settingService,
            IShipmentService shipmentService,
            IStateProvinceService stateProvinceService,
            IVendorService vendorService,
            IWorkContext workContext,
            DMSSettings dMSSettings,
            MeasureSettings measureSettings,
            VendorSettings vendorSettings)
        {
            _addressSettings = addressSettings;
            _addressService = addressService;
            _catalogSettings = catalogSettings;
            _countryService = countryService;
            _addressAttributeFormatter = addressAttributeFormatter;
            _currencyService = currencyService;
            _htmlFormatter = htmlFormatter;
            _languageService = languageService;
            _localizationService = localizationService;
            _measureService = measureService;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _qrCodeService = qrCodeService;
            _settingService = settingService;
            _shipmentService = shipmentService;
            _stateProvinceService = stateProvinceService;
            _vendorService = vendorService;
            _workContext = workContext;
            _dMSSettings = dMSSettings;
            _measureSettings = measureSettings;
            _vendorSettings = vendorSettings;
        }

        #endregion

        #region Utilities

        protected virtual async Task<AddressItem> GetShippingAddressAsync(Language lang, Order order)
        {
            var addressResult = new AddressItem();

            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                if (!order.PickupInStore)
                {
                    if (order.ShippingAddressId == null || await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value) is not Address shippingAddress)
                        throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    if (!string.IsNullOrEmpty(shippingAddress.Company))
                        addressResult.Company = shippingAddress.Company;

                    addressResult.Name = $"{shippingAddress.FirstName} {shippingAddress.LastName}";

                    if (_addressSettings.PhoneEnabled)
                        addressResult.Phone = shippingAddress.PhoneNumber;

                    if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(shippingAddress.FaxNumber))
                        addressResult.Fax = shippingAddress.FaxNumber;

                    if (_addressSettings.StreetAddressEnabled)
                        addressResult.Address = shippingAddress.Address1;

                    if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(shippingAddress.Address2))
                        addressResult.Address2 = shippingAddress.Address2;

                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                        _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                    {
                        addressResult.AddressLine = $"{shippingAddress.City}, " +
                            $"{(!string.IsNullOrEmpty(shippingAddress.County) ? $"{shippingAddress.County}, " : string.Empty)}" +
                            $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                            $"{shippingAddress.ZipPostalCode}";
                    }

                    if (_addressSettings.CountryEnabled && await _countryService.GetCountryByAddressAsync(shippingAddress) is Country country)
                    {
                        addressResult.Country = await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id);
                    }

                    //custom attributes
                    var customShippingAddressAttributes = await _addressAttributeFormatter
                        .FormatAttributesAsync(shippingAddress.CustomAttributes, "<br />");
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        var text = _htmlFormatter.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true);
                        addressResult.AddressAttributes = text.Split('\n').ToList();
                    }
                }
                else if (order.PickupAddressId.HasValue && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    if (!string.IsNullOrEmpty(pickupAddress.Address1))
                        addressResult.Address = pickupAddress.Address1;

                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                        _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                    {
                        addressResult.AddressLine = $"{pickupAddress.City}, " +
                            $"{(!string.IsNullOrEmpty(pickupAddress.County) ? $"{pickupAddress.County}, " : string.Empty)}" +
                            $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(pickupAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                            $"{pickupAddress.ZipPostalCode}";
                    }

                    if (await _countryService.GetCountryByAddressAsync(pickupAddress) is Country country)
                        addressResult.Country = await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id);
                }

                addressResult.ShippingMethod = order.ShippingMethod;
            }

            return addressResult;
        }

        protected virtual async Task<List<ProductItem>> GetOrderProductItemsAsync(Order order, IList<OrderItem> orderItems, Language language, IList<ShipmentItem> shipmentItems = null)
        {
            var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? await _vendorService.GetVendorsByProductIdsAsync(orderItems.Select(item => item.ProductId).ToArray()) : new List<Vendor>();

            var result = new List<ProductItem>();

            foreach (var oi in orderItems)
            {
                var productItem = new ProductItem();
                var product = await _productService.GetProductByIdAsync(oi.ProductId);
                productItem.Name = await _localizationService.GetLocalizedAsync(product, x => x.Name, language.Id);

                if (!string.IsNullOrEmpty(oi.AttributeDescription))
                {
                    var attributes = _htmlFormatter.ConvertHtmlToPlainText(oi.AttributeDescription, true, true);
                    productItem.ProductAttributes = attributes.Split('\n').ToList();
                }

                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                    productItem.Sku = await _productService.FormatSkuAsync(product, oi.AttributesXml);

                if (_vendorSettings.ShowVendorOnOrderDetailsPage)
                    productItem.VendorName = vendors.FirstOrDefault(v => v.Id == product.VendorId)?.Name ?? string.Empty;

                string unitPrice;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    var unitPriceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(oi.UnitPriceInclTax, order.CurrencyRate);
                    unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, language.Id, true);
                }
                else
                {
                    var unitPriceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(oi.UnitPriceExclTax, order.CurrencyRate);
                    unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, language.Id, false);
                }

                productItem.Price = unitPrice;
                productItem.Quantity = shipmentItems is null ?
                    oi.Quantity.ToString() :
                    shipmentItems.FirstOrDefault(x => x.OrderItemId == oi.Id).Quantity.ToString();

                string subTotal;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    var priceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(oi.PriceInclTax, order.CurrencyRate);
                    subTotal = await _priceFormatter.FormatPriceAsync(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        language.Id, true);
                }
                else
                {
                    var priceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(oi.PriceExclTax, order.CurrencyRate);
                    subTotal = await _priceFormatter.FormatPriceAsync(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        language.Id, false);
                }

                productItem.Total = subTotal;

                result.Add(productItem);
            }

            return result;
        }

        protected virtual PageSize GetPaperSize()
        {
            var paperSizeId = _dMSSettings.DefaultPackagingSlipPaperSizeId;
            var paperSize = PageSizes.A4;

            #region Setting papaer size based on setting (Default size is A4)

            if (paperSizeId == (int)PaperSizeEnum.A0)
                paperSize = PageSizes.A0;
            else if (paperSizeId == (int)PaperSizeEnum.A1)
                paperSize = PageSizes.A1;
            else if (paperSizeId == (int)PaperSizeEnum.A2)
                paperSize = PageSizes.A2;
            else if (paperSizeId == (int)PaperSizeEnum.A3)
                paperSize = PageSizes.A3;
            else if (paperSizeId == (int)PaperSizeEnum.A4)
                paperSize = PageSizes.A4;
            else if (paperSizeId == (int)PaperSizeEnum.A5)
                paperSize = PageSizes.A5;
            else if (paperSizeId == (int)PaperSizeEnum.A6)
                paperSize = PageSizes.A6;
            else if (paperSizeId == (int)PaperSizeEnum.A7)
                paperSize = PageSizes.A7;
            else if (paperSizeId == (int)PaperSizeEnum.A9)
                paperSize = PageSizes.A9;
            else if (paperSizeId == (int)PaperSizeEnum.A0)
                paperSize = PageSizes.A0;
            else if (paperSizeId == (int)PaperSizeEnum.ArchA)
                paperSize = PageSizes.ARCH_A;
            else if (paperSizeId == (int)PaperSizeEnum.ArchB)
                paperSize = PageSizes.ARCH_B;
            else if (paperSizeId == (int)PaperSizeEnum.ArchC)
                paperSize = PageSizes.ARCH_C;
            else if (paperSizeId == (int)PaperSizeEnum.ArchD)
                paperSize = PageSizes.ARCH_D;
            else if (paperSizeId == (int)PaperSizeEnum.ArchE)
                paperSize = PageSizes.ARCH_E;
            else if (paperSizeId == (int)PaperSizeEnum.B1)
                paperSize = PageSizes.B1;
            else if (paperSizeId == (int)PaperSizeEnum.B2)
                paperSize = PageSizes.B2;
            else if (paperSizeId == (int)PaperSizeEnum.B3)
                paperSize = PageSizes.B3;
            else if (paperSizeId == (int)PaperSizeEnum.B4)
                paperSize = PageSizes.B4;
            else if (paperSizeId == (int)PaperSizeEnum.B5)
                paperSize = PageSizes.B5;
            else if (paperSizeId == (int)PaperSizeEnum.B6)
                paperSize = PageSizes.B6;
            else if (paperSizeId == (int)PaperSizeEnum.B7)
                paperSize = PageSizes.B7;
            else if (paperSizeId == (int)PaperSizeEnum.B8)
                paperSize = PageSizes.B8;
            else if (paperSizeId == (int)PaperSizeEnum.B9)
                paperSize = PageSizes.B9;
            else if (paperSizeId == (int)PaperSizeEnum.B10)
                paperSize = PageSizes.B10;
            else if (paperSizeId == (int)PaperSizeEnum.Executive)
                paperSize = PageSizes.Executive;
            else if (paperSizeId == (int)PaperSizeEnum.Legal)
                paperSize = PageSizes.Legal;
            else if (paperSizeId == (int)PaperSizeEnum.Letter)
                paperSize = PageSizes.Letter;

            #endregion

            if (_dMSSettings.PrintPackagingSlipLandscape)
                paperSize = paperSize.Landscape();

            return paperSize;
        }

        #endregion

        #region Methods

        public virtual async Task PrintPackagingSlipsToPdfAsync(Stream stream, IList<Shipment> shipments, int languageId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (shipments == null)
                throw new ArgumentNullException(nameof(shipments));

            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true);
            var language = await _languageService.GetLanguageByIdAsync(languageId);
            foreach (var shipment in shipments)
            {
                var entryName = $"{await _localizationService.GetResourceAsync("Admin.NopStation.DMS.PackagingSlips.EntryName")}{shipment.Id}";

                await using var fileStreamInZip = archive.CreateEntry($"{entryName}.pdf").Open();
                await using var pdfStream = new MemoryStream();
                await PrintPackagingSlipToPdfAsync(pdfStream, shipment, language);

                pdfStream.Position = 0;
                await pdfStream.CopyToAsync(fileStreamInZip);
            }
        }

        public virtual async Task PrintPackagingSlipToPdfAsync(Stream stream, Shipment shipment, Language language = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var pageSize = GetPaperSize();
            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

            var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(order.StoreId);

            language ??= await _languageService.GetLanguageByIdAsync(order.CustomerLanguageId);

            if (language?.Published != true)
                language = await _workContext.GetWorkingLanguageAsync();

            var shipmentItems = await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id);

            if (shipmentItems?.Any() != true)
                return;

            var orderItems = await shipmentItems
                .SelectAwait(async si => await _orderService.GetOrderItemByIdAsync(si.OrderItemId))
                .Where(pi => pi != null)
                .ToListAsync();

            if (orderItems?.Any() != true)
                return;

            var qrCode = await _qrCodeService.GenetareQrCodeInBitMapAsync(shipment.Id.ToString());

            var totalWeight = shipment.TotalWeight;
            var weightInfo = "";
            if (totalWeight != null)
                weightInfo = $"{totalWeight:F2} [{(await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name}]";

            var source = new PackagingSlipsSource
            {
                PageSize = pageSize,
                Language = language,
                FontFamily = pdfSettingsByStore.FontFamily,
                ShipmentNumberText = shipment.Id.ToString(),
                OrderNumberText = order.CustomOrderNumber,
                Address = await GetShippingAddressAsync(language, order),
                Products = _dMSSettings.PrintProductsOnPackagingSlip ? await GetOrderProductItemsAsync(order, orderItems, language, shipmentItems) : new List<ProductItem>(),
                QRimage = qrCode,
                WeightInfo = _dMSSettings.PrintWeightInfoOnPackagingSlip ? weightInfo : "",
            };

            await using var pdfStream = new MemoryStream();

            new PackagingSlipsDocument(source, _localizationService)
                .GeneratePdf(pdfStream);

            pdfStream.Position = 0;
            await pdfStream.CopyToAsync(stream);
        }

        #endregion
    }
}
