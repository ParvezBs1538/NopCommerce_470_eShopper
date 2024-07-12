using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.UrlShortener.Areas.Admin.Models.Shortenurls
{
    public record ShortenUrlModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.UrlShortener.ShortenUrl.UrlRecordId")]
        public int UrlRecordId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.UrlShortener.ShortenUrl.Slug")]
        public string Slug { get; set; }

        [NopResourceDisplayName("Admin.NopStation.UrlShortener.ShortenUrl.EntityName")]
        public string EntityName { get; set; }


        [NopResourceDisplayName("Admin.NopStation.UrlShortener.ShortenUrl.ShortUrl")]
        public string ShortUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.UrlShortener.ShortenUrl.Hash")]
        public string Hash { get; set; }

        [NopResourceDisplayName("Admin.NopStation.UrlShortener.ShortenUrl.GlobalHash")]
        public string GlobalHash { get; set; }

        [NopResourceDisplayName("Admin.NopStation.UrlShortener.ShortenUrl.NewHash")]
        public int NewHash { get; set; }
    }
}