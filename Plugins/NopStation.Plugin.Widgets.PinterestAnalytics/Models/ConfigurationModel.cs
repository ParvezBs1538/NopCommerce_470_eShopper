using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.PinterestAnalytics.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            CustomEventSearchModel = new CustomEventSearchModel();
        }
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.PinterestId")]
        public string PinterestId { get; set; }
        public bool PinterestId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.TrackingScript")]
        public string TrackingScript { get; set; }
        public bool TrackingScript_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.AdAccountId")]
        public string Ad_Account_Id { get; set; }
        public bool Ad_Account_Id_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Token")]
        public string AccessToken { get; set; }
        public bool AccessToken_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.SaveLog")]
        public bool SaveLog { get; set; }
        public bool SaveLog_OverrideForStore { get; set; }

        public bool HideCustomEventsSearch { get; set; }
        public CustomEventSearchModel CustomEventSearchModel { get; set; }
    }
}