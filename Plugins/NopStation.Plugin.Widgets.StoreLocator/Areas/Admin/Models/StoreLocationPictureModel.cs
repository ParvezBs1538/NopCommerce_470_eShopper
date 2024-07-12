using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Models
{
    public partial record StoreLocationPictureModel : BaseNopEntityModel
    {
        public int StoreLocationId { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Fields.Picture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Fields.Picture")]
        public string PictureUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Fields.OverrideAltAttribute")]
        public string OverrideAltAttribute { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.Pictures.Fields.OverrideTitleAttribute")]
        public string OverrideTitleAttribute { get; set; }
    }
}