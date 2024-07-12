using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.GoogleAnalytics.Models
{
    public class Event
    {
        public string name { get; set; }
        public object @params { get; set; }
    }
    public class GoogleAnalyticsRequest
    {
        public string client_id { get; set; }
        public List<Event> events { get; set; }
    }

    public class ProductItem
    {
        public string item_id { get; set; }
        public string item_name { get; set; }
        public string affiliation { get; set; }
        public decimal discount { get; set; } = 0.0M;
        public string item_brand { get; set; } = "";
        public string item_category { get; set; } = "";
        public string item_variant { get; set; } = "";
        public decimal price { get; set; }
        public int quantity { get; set; } = 1;
    }
}
