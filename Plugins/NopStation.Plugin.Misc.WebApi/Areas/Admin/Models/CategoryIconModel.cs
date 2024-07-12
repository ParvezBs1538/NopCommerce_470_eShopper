using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Models
{
    public record CategoryIconModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.WebApi.CategoryIcons.Fields.Category")]
        public int CategoryId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.CategoryIcons.Fields.Category")]
        public string CategoryName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.CategoryIcons.Fields.CategoryBanner")]
        [UIHint("Picture")]
        public int CategoryBannerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.CategoryIcons.Fields.CategoryBanner")]
        public string CategoryBannerUrl { get; set; }
    }
}
