using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.Widgets.StoreLocator.Models
{
    public record StoreLocationDetailsModel : BaseNopEntityModel
    {
        public StoreLocationDetailsModel()
        {
            Pictures = new List<PictureModel>();
        }

        public IList<PictureModel> Pictures { get; set; }

        public string FormattedAddress { get; set; }

        public string FullAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string County { get; set; }

        public string ZipPostalCode { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string FullDescription { get; set; }

        public string OpeningHours { get; set; }

        public string GoogleMapLocation { get; set; }

        public string SeName { get; set; }
    }
}
