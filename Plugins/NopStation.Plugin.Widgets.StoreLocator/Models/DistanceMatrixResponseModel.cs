using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Widgets.StoreLocator.Models
{
    public class DistanceMatrixResponseModel
    {
        public DistanceMatrixResponseModel()
        {
            Rows = new List<Row>();
            DestinationAddresses = new List<string>();
            OriginAddresses = new List<string>();
        }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "origin_addresses")]
        public IList<string> OriginAddresses { get; set; }

        [JsonProperty(PropertyName = "destination_addresses")]
        public IList<string> DestinationAddresses { get; set; }

        [JsonProperty(PropertyName = "rows")]
        public IList<Row> Rows { get; set; }

        public class Duration
        {
            [JsonProperty(PropertyName = "value")]
            public int Value { get; set; }

            [JsonProperty(PropertyName = "Text")]
            public string Text { get; set; }
        }

        public class Distance
        {
            [JsonProperty(PropertyName = "value")]
            public int Value { get; set; }

            [JsonProperty(PropertyName = "Text")]
            public string Text { get; set; }
        }

        public class Element
        {
            public Element()
            {
                Duration = new Duration();
                Distance = new Distance();
            }

            [JsonProperty(PropertyName = "status")]
            public string Status { get; set; }

            [JsonProperty(PropertyName = "duration")]
            public Duration Duration { get; set; }

            [JsonProperty(PropertyName = "distance")]
            public Distance Distance { get; set; }
        }

        public class Row
        {
            public Row()
            {
                Elements = new List<Element>();
            }

            [JsonProperty(PropertyName = "elements")]
            public IList<Element> Elements { get; set; }
        }

    }
}
