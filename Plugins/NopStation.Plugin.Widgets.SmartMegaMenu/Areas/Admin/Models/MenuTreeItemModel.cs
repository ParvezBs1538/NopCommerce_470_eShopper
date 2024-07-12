using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;

public class MenuTreeItemModel
{
    public MenuTreeItemModel()
    {
        Children = new List<MenuTreeItemModel>();
    }

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("children")]
    public IList<MenuTreeItemModel> Children { get; set; }
}
