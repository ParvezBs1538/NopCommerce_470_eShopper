using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public class DMSOtpService : IDMSOtpService
    {
        private readonly DMSSettings _dMSSettings;
        private readonly ICourierShipmentService _courierShipmentService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly IOTPRecordService _oTPRecordService;
        private readonly IShipmentService _shipmentService;
        private readonly IShipperService _shipperService;
        private readonly IShipmentNoteService _shipmentNoteService;
        private readonly IWorkContext _workContext;
        #region Fields



        #endregion

        #region Ctor

        public DMSOtpService(
            DMSSettings dMSSettings,
            ICourierShipmentService courierShipmentService,
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            IOrderService orderService,
            IOTPRecordService oTPRecordService,
            IShipmentService shipmentService,
            IShipperService shipperService,
            IShipmentNoteService shipmentNoteService,
            IWorkContext workContext
            )
        {
            _dMSSettings = dMSSettings;
            _courierShipmentService = courierShipmentService;
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _orderService = orderService;
            _oTPRecordService = oTPRecordService;
            _shipmentService = shipmentService;
            _shipperService = shipperService;
            _shipmentNoteService = shipmentNoteService;
            _workContext = workContext;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods

        public virtual async Task<string> GetOtpAsync(int otpLength = 4)
        {
            otpLength = otpLength < 4 || otpLength > 20 ? 4 : otpLength;
            await Task.CompletedTask;

            return CommonHelper.GenerateRandomDigitCode(otpLength);
        }

        public virtual async Task<OTPRecord> GenerateCourierVerificationOtpByCourierShipmentAsync(CourierShipment courierShipment, Shipper shipper)
        {
            if (courierShipment == null)
                throw new ArgumentNullException(nameof(courierShipment));

            if (shipper == null)
                throw new ArgumentNullException(nameof(shipper));

            var shipment = await _shipmentService.GetShipmentByIdAsync(courierShipment.ShipmentId);

            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var customer = await _workContext.GetCurrentCustomerAsync();

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var otpRecord = (await _oTPRecordService.SearchOTPRecordAsync(shipmentId: shipment.Id, deleted: false))
                            .OrderByDescending(q => q.Id).FirstOrDefault();

            if (otpRecord != null)
                return otpRecord;

            otpRecord = new OTPRecord()
            {
                ShipmentId = shipment.Id,
                CustomerId = order.CustomerId,
                OrderId = order.Id,
                AuthenticationCode = await GetOtpAsync(_dMSSettings.OtpLength),
                CreatedOnUtc = DateTime.UtcNow,
                Verified = false,
                Deleted = false,
            };
            await _oTPRecordService.InsertOTPRecordAsync(otpRecord);

            var note = (await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.Otp.OrderNote")) + $"{courierShipment.ShipmentId} : {otpRecord.AuthenticationCode}";
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = note,
                DisplayToCustomer = true,
                CreatedOnUtc = DateTime.UtcNow
            });

            var otpNote = string.Format(await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.ProofOfDelivery.Otp.CustomerOtp"), shipment.Id, otpRecord.AuthenticationCode);
            var shipmentNote = new ShipmentNote()
            {
                CourierShipmentId = courierShipment.Id,
                NopShipmentId = shipment.Id,
                Note = otpNote,
                DisplayToCustomer = true,
                UpdatedByCustomerId = shipper.CustomerId
            };
            await _shipmentNoteService.InsertShipmentNoteAsync(shipmentNote);

            return otpRecord;
        }


        public virtual async Task<bool> CheckAndVerifyProofOfDeliveryOtpAsync(Shipper shipper, int shipmentId, string otp)
        {
            if (shipper == null)
                throw new ArgumentNullException(nameof(shipper));

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);

            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var otpRecord = (await _oTPRecordService.SearchOTPRecordAsync(shipmentId: shipment.Id, deleted: false))
                            .OrderByDescending(q => q.Id).FirstOrDefault();

            if (otpRecord != null)
                throw new ArgumentNullException(nameof(otpRecord));

            if (otpRecord.Verified)
            {
                var msg = $"CheckAndVerifyProofOfDeliveryOtp: otp is already verified for the shipmentId: {shipmentId}";
                await _logger.InsertLogAsync(LogLevel.Error, msg, msg);

                _notificationService.WarningNotification("Otp already verified");
            }

            if (otpRecord.AuthenticationCode == otp)
            {
                otpRecord.Verified = true;
                otpRecord.VerifiedByShipperId = shipper.Id;
                otpRecord.VerifiedOnUtc = DateTime.UtcNow;
                await _oTPRecordService.UpdateOTPRecordAsync(otpRecord);

                return true;
            }

            return false;
        }



        #endregion
    }
}
