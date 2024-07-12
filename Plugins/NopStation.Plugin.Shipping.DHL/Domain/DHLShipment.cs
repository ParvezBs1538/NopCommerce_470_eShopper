using Nop.Core;

namespace NopStation.Plugin.Shipping.DHL.Domain
{
    public class DHLShipment : BaseEntity
    {
        public int OrderId { get; set; }

        public string MessageReference { get; set; }

        public string GlobalProductCode { get; set; }

        public string AirwayBillNumber { get; set; }

        public string ShippingLabelBase64Pdf { get; set; }
    }
}
