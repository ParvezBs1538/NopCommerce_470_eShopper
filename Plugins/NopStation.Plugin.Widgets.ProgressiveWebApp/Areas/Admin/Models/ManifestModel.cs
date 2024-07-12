using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models
{
    public class ManifestModel
    {
        public ManifestModel()
        {
            Icons = new List<ManifestIconModel>();
        }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "short_name")]
        public string ShortName { get; set; }

        [JsonProperty(PropertyName = "theme_color")]
        public string ThemeColor { get; set; }

        [JsonProperty(PropertyName = "background_color")]
        public string BackgroundColor { get; set; }

        [JsonProperty(PropertyName = "display")]
        public string Display { get; set; }

        [JsonProperty(PropertyName = "start_url")]
        public string StartUrl { get; set; }

        [JsonProperty(PropertyName = "icons")]
        public IList<ManifestIconModel> Icons { get; set; }

        [JsonProperty(PropertyName = "splash_pages")]
        public string SplashPages { get; set; }

        [JsonProperty(PropertyName = "scope")]
        public string ApplicationScope { get; set; }

        public class ManifestIconModel
        {
            [JsonProperty(PropertyName = "src")]
            public string Source { get; set; }

            [JsonProperty(PropertyName = "sizes")]
            public string Sizes { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }
        }
    }
}
