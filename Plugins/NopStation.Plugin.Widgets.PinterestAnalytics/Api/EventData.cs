namespace NopStation.Plugin.Widgets.PinterestAnalytics.Api
{
    public class EventData
    {
        public string Event_Name { get; set; }
        public string Action_Source { get; set; }
        public long Event_Time { get; set; }
        public string Event_Id { get; set; }
        public UserData User_Data { get; set; }
        public CustomData Custom_Data { get; set; }
    }
}
