using System;
using System.Collections.Generic;
using System.Text;

namespace NopStation.Plugin.Widgets.CategoryBanners.Services.Cache
{
    public class ModelCacheDefaults
    {
        public static string CategoryBannerPictureModelKey => "Nopstation.categorybanner.detailspictures-{0}-{1}-{2}-{3}-{4}-{5}";
        public static string CategoryBannerPicturePrefixCacheKey => "Nopstation.categorybanner.detailspictures";
        public static string CategoryBannerPicturePrefixCacheKeyById => "Nopstation.categorybanner.detailspictures-{0}-";
    }
}
