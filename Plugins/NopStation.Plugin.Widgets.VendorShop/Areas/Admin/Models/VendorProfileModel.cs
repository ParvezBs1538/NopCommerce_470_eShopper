using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models
{
    public partial record VendorProfileModel : BaseNopEntityModel, ILocalizedModel<VendorProfileLocalizedModel>
    {
        public int VendorId { get; set; }
        [NopResourceDisplayName("Admin.NopStation.VendorShop.VendorProfile.Fields.Description")]
        public string Description { get; set; }
        [NopResourceDisplayName("Admin.NopStation.VendorShop.VendorProfile.Fields.CustomCss")]
        public string CustomCss { get; set; }
        public bool Description_OverrideForStore { get; set; }
        [UIHint("picture")]
        [NopResourceDisplayName("Admin.NopStation.VendorShop.VendorProfile.Fields.ProfilePictureId")]
        public int ProfilePictureId { get; set; }
        public bool ProfilePictureId_OverrideForStore { get; set; }
        [UIHint("picture")]
        [NopResourceDisplayName("Admin.NopStation.VendorShop.VendorProfile.Fields.BannerPictureId")]
        public int BannerPictureId { get; set; }
        public bool BannerPictureId_OverrideForStore { get; set; }
        [UIHint("picture")]
        [NopResourceDisplayName("Admin.NopStation.VendorShop.VendorProfile.Fields.MobileBannerPictureId")]
        public int MobileBannerPictureId { get; set; }
        public bool MobileBannerPictureId_OverrideForStore { get; set; }
        public int ActiveStoreScopeConfiguration { get; set; }
        public IList<VendorProfileLocalizedModel> Locales { get; set; }

        public VendorProfileModel()
        {
            Locales = new List<VendorProfileLocalizedModel>();
        }
    }

    public partial record VendorProfileLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.VendorProfile.Fields.Description")]
        public string Description { get; set; }

    }
}
