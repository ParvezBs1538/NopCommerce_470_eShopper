using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public interface IQrCodeService
    {
        Task<byte[]> GenetareQrCodeInBitMapAsync(string qrText);

        Task<byte[]> GenetareShippingQrCodeInBitMapAsync(Shipment shipment, Order order, Address shippingAddress,
            Customer customer, string customerName = null, string customerCountry = null, string customerStateProvince = null,
            string totalWeight = null);

        Task<byte[]> GenetareShippingQrCodeInByteAsync(Shipment shipment, Order order, Address shippingAddress,
            Customer customer, string customerName = null, string customerCountry = null, string customerStateProvince = null,
            string totalWeight = null);

    }
}
