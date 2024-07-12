using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Configuration.Fields.CaptchaEnabled")]
        public bool CaptchaEnabled { get; set; }
        public bool CaptchaEnabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Configuration.Fields.MinimumIntervalToSubmitSurvey")]
        public int MinimumIntervalToSubmitSurvey { get; set; }
        public bool MinimumIntervalToSubmitSurvey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Configuration.Fields.ImageSquarePictureSize")]
        public int ImageSquarePictureSize { get; set; }
        public bool ImageSquarePictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Configuration.Fields.CountDisplayedYearsDatePicker")]
        public int CountDisplayedYearsDatePicker { get; set; }
        public bool CountDisplayedYearsDatePicker_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}