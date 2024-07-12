using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.PinterestAnalytics.Models
{
    public partial record CustomEventSearchModel : BaseSearchModel
    {
        #region Ctor

        public CustomEventSearchModel()
        {
            AddCustomEvent = new CustomEventModel();
        }

        #endregion

        #region Properties

        public int StoreId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Configuration.CustomEvents.Search.WidgetZone")]
        public string WidgetZone { get; set; }

        public CustomEventModel AddCustomEvent { get; set; }

        #endregion
    }
}