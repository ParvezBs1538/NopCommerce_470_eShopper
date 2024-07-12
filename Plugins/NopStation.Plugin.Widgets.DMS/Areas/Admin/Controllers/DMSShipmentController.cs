using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Controllers
{
    public class DMSShipmentController : NopStationAdminController
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDMSPdfService _dMSPdfService;
        private readonly IDMSShipmentModelFactory _dMSShipmentModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly IPdfService _pdfService;
        private readonly IPermissionService _permissionService;
        private readonly IQrCodeService _qrCodeService;
        private readonly ISettingService _settingService;
        private readonly IShipmentService _shipmentService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly ILanguageService _languageService;



        #endregion

        #region Ctor

        public DMSShipmentController(
            IDateTimeHelper dateTimeHelper,
            IDMSPdfService dMSPdfService,
            IDMSShipmentModelFactory dMSShipmentModelFactory,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IOrderService orderService,
            IPdfService pdfService,
            IPermissionService permissionService,
            IQrCodeService qrCodeService,
            ISettingService settingService,
            IShipmentService shipmentService,
            IStoreContext storeContext,
            IWorkContext workContext,
            OrderSettings orderSettings,
            ILanguageService languageService)
        {
            _dateTimeHelper = dateTimeHelper;
            _dMSPdfService = dMSPdfService;
            _dMSShipmentModelFactory = dMSShipmentModelFactory;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _orderService = orderService;
            _pdfService = pdfService;
            _permissionService = permissionService;
            _qrCodeService = qrCodeService;
            _settingService = settingService;
            _shipmentService = shipmentService;
            _storeContext = storeContext;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _languageService = languageService;
        }

        #endregion

        #region Utilites


        protected virtual async ValueTask<bool> HasAccessToOrderAsync(Order order)
        {
            return order != null && await HasAccessToOrderAsync(order.Id);
        }

        protected virtual async Task<bool> HasAccessToOrderAsync(int orderId)
        {
            if (orderId == 0)
                return false;

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null)
                //not a vendor; has access
                return true;

            var vendorId = currentVendor.Id;
            var hasVendorProducts = (await _orderService.GetOrderItemsAsync(orderId, vendorId: vendorId)).Any();

            return hasVendorProducts;
        }

        protected virtual async ValueTask<bool> HasAccessToShipmentAsync(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            if (await _workContext.GetCurrentVendorAsync() == null)
                //not a vendor; has access
                return true;

            return await HasAccessToOrderAsync(shipment.OrderId);
        }


        #endregion

        #region Methods
        //[HttpGet("{DMSShipment/GetShipmentQrcode/shipmentId}")]
        public async Task<IActionResult> GetShipmentQrcode(int id)
        {
            var bytes = await _dMSShipmentModelFactory.GeneratePackagingSlipsToPdfAsync(id);

            if (bytes == null)
            {
                return NotFound();
            }

            return File(bytes, MimeTypes.ImagePng, $"packagingslip_{id}.png");
        }

        public virtual async Task<IActionResult> DMSPdfPackagingSlip(int shipmentId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
            if (shipment == null)
                return RedirectToAction("List");
            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return RedirectToAction("List");
            var lang = _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? await _languageService.GetLanguageByIdAsync(0) : (await _workContext.GetWorkingLanguageAsync());

            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _dMSPdfService.PrintPackagingSlipToPdfAsync(stream, shipment, lang);
                bytes = stream.ToArray();
            }
            var fileName = $"{await _localizationService.GetResourceAsync("Admin.NopStation.DMS.PackagingSlips.PDF.EntryName")}{shipmentId}";
            return File(bytes, MimeTypes.ApplicationPdf, $"{fileName}.pdf");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DMSPdfPackagingSlipSelected(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                shipments.AddRange(await _shipmentService.GetShipmentsByIdsAsync(ids));
            }
            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null)
            {
                shipments = await shipments.WhereAwait(HasAccessToShipmentAsync).ToListAsync();
            }

            try
            {
                byte[] bytes;
                await using (var stream = new MemoryStream())
                {
                    await _dMSPdfService.PrintPackagingSlipsToPdfAsync(stream, shipments, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf, "packagingslips.zip");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("ShipmentList");
            }
        }


        [HttpPost, ActionName("DMSPdfShipmentList")]
        [FormValueRequired("ns-dms-exportpackagingslips-all")]
        public virtual async Task<IActionResult> PdfPackagingSlipAll(ShipmentSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var startDateValue = model.StartDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            var endDateValue = model.EndDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            var vendorId = 0;
            if (currentVendor != null)
                vendorId = currentVendor.Id;

            //load shipments
            var shipments = await _shipmentService.GetAllShipmentsAsync(vendorId: vendorId,
                warehouseId: model.WarehouseId,
                shippingCountryId: model.CountryId,
                shippingStateId: model.StateProvinceId,
                shippingCounty: model.County,
                shippingCity: model.City,
                trackingNumber: model.TrackingNumber,
                loadNotShipped: model.LoadNotShipped,
                loadNotReadyForPickup: model.LoadNotReadyForPickup,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue);

            //ensure that we at least one shipment selected
            if (!shipments.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.Shipments.NoShipmentsSelected"));
                return RedirectToAction("ShipmentList");
            }

            try
            {
                byte[] bytes;
                await using (var stream = new MemoryStream())
                {
                    await _dMSPdfService.PrintPackagingSlipsToPdfAsync(stream, shipments, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf, "packagingslips.zip");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("ShipmentList", "Order");
            }
        }


        #endregion
    }
}
