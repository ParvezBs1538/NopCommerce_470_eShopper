namespace NopStation.Plugin.Shipping.DHL.Areas.Admin.Models
{
    public class DHLShipmentValidationResponseModel
    {
        public string AirwayBillNumber { get; set; }

        public string ShippingLabelBase64Pdf { get; set; }

        public string MessageReference { get; set; }

        public string Error { get; set; }
    }
}
