using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NopStation.Plugin.AddressValidator.Google.Services.Models
{
    public class GeocodingResponse
    {
        public GeocodingResponse()
        {
            Results = new List<Result>();
        }

        [JsonProperty("results")]
        public List<Result> Results { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }


        public class AddressComponent
        {
            public AddressComponent()
            {
                Types = new List<string>();
            }

            [JsonProperty("long_name")]
            public string LongName { get; set; }

            [JsonProperty("short_name")]
            public string ShortName { get; set; }

            [JsonProperty("types")]
            public List<string> Types { get; set; }
        }

        public class Location
        {
            [JsonProperty("lat")]
            public double Latitude { get; set; }

            [JsonProperty("lng")]
            public double Longitude { get; set; }
        }

        public class Northeast
        {
            [JsonProperty("lat")]
            public double Latitude { get; set; }

            [JsonProperty("lng")]
            public double Longitude { get; set; }
        }

        public class Southwest
        {
            [JsonProperty("lat")]
            public double Latitude { get; set; }

            [JsonProperty("lng")]
            public double Longitude { get; set; }
        }

        public class Viewport
        {
            public Viewport()
            {
                Northeast = new Northeast();
                Southwest = new Southwest();
            }

            [JsonProperty("northeast")]
            public Northeast Northeast { get; set; }

            [JsonProperty("southwest")]
            public Southwest Southwest { get; set; }
        }

        public class Geometry
        {
            public Geometry()
            {
                Location = new Location();
                Viewport = new Viewport();
            }

            [JsonProperty("location")]
            public Location Location { get; set; }

            [JsonProperty("location_type")]
            public string LocationType { get; set; }

            [JsonProperty("viewport")]
            public Viewport Viewport { get; set; }
        }

        public class PlusCode
        {
            [JsonProperty("compound_code")]
            public string CompoundCode { get; set; }

            [JsonProperty("global_code")]
            public string GlobalCode { get; set; }
        }

        public class Result
        {
            public Result()
            {
                AddressComponents = new List<AddressComponent>();
                Geometry = new Geometry();
                PlusCode = new PlusCode();
                Types = new List<string>();
            }

            [JsonProperty("address_components")]
            public List<AddressComponent> AddressComponents { get; set; }

            [JsonProperty("formatted_address")]
            public string FormattedAddress { get; set; }

            [JsonProperty("geometry")]
            public Geometry Geometry { get; set; }

            [JsonProperty("place_id")]
            public string PlaceId { get; set; }

            [JsonProperty("plus_code")]
            public PlusCode PlusCode { get; set; }

            [JsonProperty("types")]
            public List<string> Types { get; set; }
        }
    }
}
