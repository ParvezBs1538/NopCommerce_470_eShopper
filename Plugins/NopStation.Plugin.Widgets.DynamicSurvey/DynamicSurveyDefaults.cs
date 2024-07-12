using Nop.Core.Caching;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey
{
    public class DynamicSurveyDefaults
    {
        public const string SYSTEM_NAME = "NopStation.Plugin.Widgets.DynamicSurvey";

        public static string OutputPath => "~/Plugins/" + SYSTEM_NAME;

        public static string SurveyAttributePrefix => "survey_attribute_";

        public static CacheKey SurveyAttributeMappingsBySurveyCacheKey => new("Nopstation.surveyattributemapping.bysurvey.{0}");

        public static CacheKey SurveyAttributeValuesByAttributeCacheKey => new("Nopstation.surveyattributevalue.byattribute.{0}");

        public static CacheKey PredefinedSurveyAttributeValuesByAttributeCacheKey => new("Nopstation.predefinedsurveyattributevalue.byattribute.{0}");

        public static CacheKey SurveysAllCacheKey => new("Nopstation.survey.all.{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}", NopEntityCacheDefaults<Survey>.AllPrefix);

        public static CacheKey SurveyBySystemNameCacheKey => new("Nopstation.survey.bysystemname.{0}-{1}-{2}", SurveyBySystemNamePrefix);

        public static string SurveyBySystemNamePrefix => "Nopstation.survey.bysystemname.{0}";

        public static CacheKey SurveyAttributeImageSquarePictureModelKey => new("Nopstation.pres.surveyattribute.imagesquare.picture-{0}-{1}-{2}", SurveyAttributeImageSquarePicturePrefixCacheKey);

        public static string SurveyAttributeImageSquarePicturePrefixCacheKey => "Nopstation.pres.surveyattribute.imagesquare.picture";
    }
}