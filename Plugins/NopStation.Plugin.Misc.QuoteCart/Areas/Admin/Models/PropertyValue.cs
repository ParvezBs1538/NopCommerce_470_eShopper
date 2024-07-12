using System.Text.Json.Serialization;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public class PropertyValue
{
    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("selected")]
    public bool Selected { get; set; }
}
