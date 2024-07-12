using Nop.Core;

namespace NopStation.Plugin.Misc.UrlShortener.Domains
{
    public class ShortenUrl : BaseEntity
    {
        public int UrlRecordId { get; set; }

        public string Slug { get; set; }

        public string ShortUrl { get; set; }

        public string Hash { get; set; }

        public string GlobalHash { get; set; }

        public int NewHash { get; set; }

        public bool Deleted { get; set; }
    }
}
