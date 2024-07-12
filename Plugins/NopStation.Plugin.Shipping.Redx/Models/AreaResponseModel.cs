using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Shipping.Redx.Models
{
    public class AreaResponseModel
    {
        public AreaResponseModel()
        {
            Areas = new List<AreaModel>();
        }

        public List<AreaModel> Areas { get; set; }

        public class AreaModel
        {
            [JsonProperty("Id")]
            public int RedxAreaId { get; set; }

            public string Name { get; set; }

            [JsonProperty("Post_Code")]
            public string PostCode { get; set; }

            [JsonProperty("District_Name")]
            public string DistrictName { get; set; }
        }
    }
}