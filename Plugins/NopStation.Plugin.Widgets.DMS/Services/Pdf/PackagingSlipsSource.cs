using System.Collections.Generic;
using System.ComponentModel;
using Nop.Services.Common.Pdf;

namespace NopStation.Plugin.Widgets.DMS.Services.Pdf
{
    public partial class PackagingSlipsSource : DocumentSource
    {
        #region Ctor
        public PackagingSlipsSource()
        {
            Products = new();
        }

        #endregion

        #region Properties
        [DisplayName("NopStation.DMS.PDFPackagingSlip.Address")]
        public AddressItem Address { get; set; }

        [DisplayName("NopStation.DMS.PDFPackagingSlip.Order")]
        public string OrderNumberText { get; set; }

        public List<ProductItem> Products { get; set; }

        [DisplayName("NopStation.DMS.PDFPackagingSlip.Shipment")]
        public string ShipmentNumberText { get; set; }

        public byte[] QRimage { get; set; }

        [DisplayName("NopStation.DMS.PDFPackagingSlip.WeightInfo")]
        public string WeightInfo { get; set; }

        #endregion
    }
}