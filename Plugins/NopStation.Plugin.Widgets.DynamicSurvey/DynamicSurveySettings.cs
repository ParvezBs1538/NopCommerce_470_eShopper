using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.DynamicSurvey
{
    public class DynamicSurveySettings : ISettings
    {
        public bool CaptchaEnabled { get; set; }

        public int MinimumIntervalToSubmitSurvey { get; set; }

        public int ImageSquarePictureSize { get; set; }

        public int CountDisplayedYearsDatePicker { get; set; }
    }
}