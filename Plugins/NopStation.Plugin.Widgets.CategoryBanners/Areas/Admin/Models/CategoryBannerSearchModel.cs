using System;
using System.Collections.Generic;
using System.Text;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.CategoryBanners.Areas.Admin.Models
{
    public record CategoryBannerSearchModel : BaseSearchModel
    {
        public CategoryBannerSearchModel()
        {
            CategoryBanner = new CategoryBannerModel();
        }

        public int CategoryId { get; set; }

        public CategoryBannerModel CategoryBanner { get; set; }
    }
}
