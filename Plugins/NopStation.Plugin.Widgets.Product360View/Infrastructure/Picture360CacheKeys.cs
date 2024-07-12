using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.Product360View.Infrastructure
{
    public class Picture360CacheKeys
    {
        public static CacheKey PicturesCacheKey => new("Nop.product360view.pictures.-{0}-{1}", Picture360Prefix);
        public static string Picture360Prefix = "Nop.product360viewpictures.{0}";

        public static CacheKey ImageSettingCacheKey = new("Nop.product360view.imagesettings-{0}", PictureSetting360Prefix);
        public static string PictureSetting360Prefix = "Nop.product360viewsettings.{0}";

        public static CacheKey PictureMappingCacheKey = new("Nop.product360view.picturemappings-{0}", PictureMapping360Prefix);
        public static string PictureMapping360Prefix = "Nop.product360viewmappings.{0}";
    }
}
