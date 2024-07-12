using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Popups.Areas.Admin.Models;

public partial record ConfigurationModel : BaseNopModel, ISettingsModel, ILocalizedModel<ConfigurationModel.ConfigurationLocalizedModel>
{
    public ConfigurationModel()
    {
        Locales = new List<ConfigurationLocalizedModel>();
    }

    [NopResourceDisplayName("Admin.NopStation.Popups.Configuration.Fields.EnableNewsletterPopup")]
    public bool EnableNewsletterPopup { get; set; }
    public bool EnableNewsletterPopup_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Configuration.Fields.NewsletterPopupTitle")]
    public string NewsletterPopupTitle { get; set; }
    public bool NewsletterPopupTitle_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Configuration.Fields.NewsletterPopupDesctiption")]
    public string NewsletterPopupDesctiption { get; set; }
    public bool NewsletterPopupDesctiption_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Configuration.Fields.BackgroundPicture")]
    [UIHint("Picture")]
    public int BackgroundPictureId { get; set; }
    public bool BackgroundPictureId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Configuration.Fields.RedirectUrl")]
    public string RedirectUrl { get; set; }
    public bool RedirectUrl_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Configuration.Fields.PopupOpenerSelector")]
    public string PopupOpenerSelector { get; set; }
    public bool PopupOpenerSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Configuration.Fields.OpenPopupOnLoadPage")]
    public bool OpenPopupOnLoadPage { get; set; }
    public bool OpenPopupOnLoadPage_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Configuration.Fields.DelayTime")]
    public int DelayTime { get; set; }
    public bool DelayTime_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Configuration.Fields.AllowCustomerToSelectDoNotShowThisPopupAgain")]
    public bool AllowCustomerToSelectDoNotShowThisPopupAgain { get; set; }
    public bool AllowCustomerToSelectDoNotShowThisPopupAgain_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Configuration.Fields.PreSelectedDoNotShowThisPopupAgain")]
    public bool PreSelectedDoNotShowThisPopupAgain { get; set; }
    public bool PreSelectedDoNotShowThisPopupAgain_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Configuration.Fields.ShowOnHomePageOnly")]
    public bool ShowOnHomePageOnly { get; set; }
    public bool ShowOnHomePageOnly_OverrideForStore { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }

    public IList<ConfigurationLocalizedModel> Locales { get; set; }

    #region Nested class

    public partial class ConfigurationLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Popups.Configuration.Fields.NewsletterPopupTitle")]
        public string NewsletterPopupTitle { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Popups.Configuration.Fields.NewsletterPopupDesctiption")]
        public string NewsletterPopupDesctiption { get; set; }
    }

    #endregion
}
