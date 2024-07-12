using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.CategoryBanners.Areas.Admin.Models
{
    public record CategoryBannerModel : BaseNopEntityModel
    {
        public int CategoryId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.ForMobile")]
        public bool ForMobile { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.OverrideTitleAttribute")]
        public string OverrideTitleAttribute { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.OverrideAltAttribute")]
        public string OverrideAltAttribute { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.Picture")]
        public int PictureId { get; set; }
        public string PictureUrl { get; set; }
    }
}
