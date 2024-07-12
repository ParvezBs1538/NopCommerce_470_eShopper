using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.PopupLogin.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.PopupLogin.Configuration.Fields.EnablePopupLogin")]
        public bool EnablePopupLogin { get; set; }
        public bool EnablePopupLogin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PopupLogin.Configuration.Fields.LoginUrlElementSelector")]
        public string LoginUrlElementSelector { get; set; }
        public bool LoginUrlElementSelector_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}