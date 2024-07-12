using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.PinterestAnalytics.Api
{
    public class CustomData
    {
        public string Currency { get; set; }
        public string Value { get; set; }
        public List<Content> Contents { get; set; }
        public List<string> Content_Ids { get; set; }
        public int Num_Items { get; set; }
    }
}
