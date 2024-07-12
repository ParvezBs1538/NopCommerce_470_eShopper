namespace NopStation.Plugin.Widgets.DMS.Models
{
    public class QrCodeShipmentInfoModel
    {
        public QrCodeShipmentInfoModel()
        {
            //ShippingAddress = new QrCodeShipmentAddressModel();
        }

        //public int CustomertId { get; set; }
        //
        //public string CustomerName { get; set; }
        //
        //public int OrderId { get; set; }
        //
        //public DateTime OrderDate { get; set; }

        public int ShipmentId { get; set; }

        public string TrackingNumber { get; set; }

        //public string TotalWeight { get; set; }

        //public QrCodeShipmentAddressModel ShippingAddress { get; set; }
    }
}
