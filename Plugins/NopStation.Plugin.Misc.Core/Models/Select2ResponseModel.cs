using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Misc.Core.Models;

public class Select2ResponseModel
{
    public Select2ResponseModel()
    {
        Results = new List<Select2Item>();
        Pagination = new Select2Pagination();
    }

    [JsonProperty("results")]
    public IList<Select2Item> Results { get; set; }

    [JsonProperty("pagination")]
    public Select2Pagination Pagination { get; set; }

    public class Select2Item
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class Select2Pagination
    {
        [JsonProperty("more")]
        public bool More { get; set; }
    }
}
