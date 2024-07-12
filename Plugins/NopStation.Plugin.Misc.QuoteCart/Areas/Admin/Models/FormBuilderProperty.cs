using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public class FormBuilderProperty
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("subtype")]
    public string Subtype { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("access")]
    public bool Access { get; set; }

    [JsonPropertyName("required")]
    public bool? Required { get; set; }

    [JsonPropertyName("className")]
    public string ClassName { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("userData")]
    public List<string> UserData { get; set; }

    [JsonPropertyName("classname")]
    public string Classname { get; set; }

    [JsonPropertyName("userdata")]
    public List<string> Userdata { get; set; }
}
