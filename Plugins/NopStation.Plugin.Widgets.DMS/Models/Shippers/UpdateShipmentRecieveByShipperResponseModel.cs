using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DMS.Models.Shippers
{
    public record UpdateShipmentRecieveByShipperResponseModel : BaseNopModel
    {
        public UpdateShipmentRecieveByShipperResponseModel()
        {
            //ShipmentAddress = new QrCodeShipmentAddressModel();
        }
        public int ShipmentId { get; set; }

        //public int OrderId { get; set; }

        public int AssignToShippierId { get; set; }

        public int ShipmentStatusId { get; set; }

        //public QrCodeShipmentAddressModel ShipmentAddress { get; set; }

    }
}
