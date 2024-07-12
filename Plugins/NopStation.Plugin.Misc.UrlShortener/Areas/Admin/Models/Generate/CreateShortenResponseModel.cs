using Newtonsoft.Json;

namespace NopStation.Plugin.Misc.UrlShortener.Areas.Admin.Models.Generate
{
    public class CreateShortenResponseModel
    {
        public CreateShortenResponseModel()
        {
            Data = new DataModel();
        }

        [JsonProperty(PropertyName = "status_code")]
        public int StatusCode { get; set; }

        [JsonProperty(PropertyName = "status_txt")]
        public string StatusText { get; set; }

        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }


        public class DataModel
        {
            [JsonProperty(PropertyName = "long_url")]
            public string LongUrl { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string ShortUrl { get; set; }

            [JsonProperty(PropertyName = "hash")]
            public string Hash { get; set; }

            [JsonProperty(PropertyName = "global_hash")]
            public string GlobalHash { get; set; }

            [JsonProperty(PropertyName = "new_hash")]
            public int NewHash { get; set; }
        }
    }
}
