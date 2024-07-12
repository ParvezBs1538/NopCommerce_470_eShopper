using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using QRCoder;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public class QrCodeService : IQrCodeService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IOrderService _orderService;
        private readonly IShipmentService _shipmentService;

        #endregion

        #region Ctor

        public QrCodeService(ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IOrderService orderService,
            IShipmentService shipmentService
            )
        {
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _orderService = orderService;
            _shipmentService = shipmentService;
        }

        #endregion

        #region Methods

        public virtual async Task<byte[]> GenetareQrCodeInBitMapAsync(string qrText)
        {
            //qrText = "asdfasdf asd fasdf asdf adfs asdfasdf ";
            if (string.IsNullOrEmpty(qrText))
                return null;

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new BitmapByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(40);
            await Task.CompletedTask;

            return qrCodeImage;
        }

        public virtual async Task<byte[]> GenetareShippingQrCodeInBitMapAsync(Shipment shipment, Order order, Address shippingAddress,
            Customer customer, string customerName = null, string customerCountry = null, string customerStateProvince = null,
            string totalWeight = null)
        {
            //if (shipmentId == 0)
            //    return null;

            //var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);

            if (shipment == null)
                return null;

            // //var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

            // if (order == null)
            //     return null;

            // if (order.PickupInStore || order.ShippingAddressId == null)
            //     throw new ArgumentException("Order is no shippable");

            //// var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            // if(customer == null)
            //     return null;

            // if(shippingAddress == null)
            //     return null;

            //var qrCodeShipmentAddress = new QrCodeShipmentAddressModel()
            //{
            //    Name = customerName ?? string.Empty,
            //    Address1 = shippingAddress.Address1,
            //    Address2 = shippingAddress.Address2,
            //    City = shippingAddress.City,
            //    County = shippingAddress.County,
            //    StateProvince = customerStateProvince ?? string.Empty,
            //    Country = customerCountry ?? string.Empty,
            //    ZipPostalCode = shippingAddress.ZipPostalCode,
            //    PhoneNumber = shippingAddress.PhoneNumber,
            //    Email = customer.Email
            //};

            //var qrCodeShipmentInfo = new QrCodeShipmentInfoModel()
            //{
            //    CustomertId = customer.Id,
            //    CustomerName = customerName ?? string.Empty,
            //    OrderId = shipment.OrderId,
            //    OrderDate = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, await _dateTimeHelper.GetCustomerTimeZoneAsync(customer)),
            //    ShipmentId = shipment.Id,
            //    TrackingNumber = shipment.TrackingNumber,
            //    TotalWeight = totalWeight,
            //    //ShippingAddress = qrCodeShipmentAddress
            //};
            var qrCodeImage = await GenetareQrCodeInBitMapAsync(shipment.Id.ToString());

            //var imagePath = $"C:\\00.Data\\Projects\\QRappTest\\Qrcodes_image\\qr_qrCodeShipmentInfo_{shipment.Id}.png";
            //qrCodeImage.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);

            return qrCodeImage;
        }

        public virtual async Task<byte[]> GenetareShippingQrCodeInByteAsync(Shipment shipment, Order order, Address shippingAddress,
            Customer customer, string customerName = null, string customerCountry = null, string customerStateProvince = null,
            string totalWeight = null)
        {
            var qrCodeByteArrayImage = await GenetareShippingQrCodeInBitMapAsync(shipment, order, shippingAddress, customer,
                customerName, customerCountry, customerStateProvince, totalWeight);

            if (qrCodeByteArrayImage == null)
                return null;

            return qrCodeByteArrayImage;
        }

        #endregion
    }
}
