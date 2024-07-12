using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services
{
    public static class SurveyAttributeExtensions
    {
        public static bool ShouldHaveValues(this SurveyAttributeMapping surveyAttributeMapping)
        {
            if (surveyAttributeMapping == null)
                return false;

            if (surveyAttributeMapping.AttributeControlType == AttributeControlType.TextBox ||
                surveyAttributeMapping.AttributeControlType == AttributeControlType.MultilineTextbox ||
                surveyAttributeMapping.AttributeControlType == AttributeControlType.Datepicker ||
                surveyAttributeMapping.AttributeControlType == AttributeControlType.FileUpload)
                return false;

            return true;
        }

        public static bool CanBeUsedAsCondition(this SurveyAttributeMapping surveyAttributeMapping)
        {
            if (surveyAttributeMapping == null)
                return false;

            if (surveyAttributeMapping.AttributeControlType == AttributeControlType.ReadonlyCheckboxes ||
                surveyAttributeMapping.AttributeControlType == AttributeControlType.TextBox ||
                surveyAttributeMapping.AttributeControlType == AttributeControlType.MultilineTextbox ||
                surveyAttributeMapping.AttributeControlType == AttributeControlType.Datepicker ||
                surveyAttributeMapping.AttributeControlType == AttributeControlType.FileUpload)
                return false;

            //other attribute control types support it
            return true;
        }

        public static bool ValidationRulesAllowed(this SurveyAttributeMapping surveyAttributeMapping)
        {
            if (surveyAttributeMapping == null)
                return false;

            if (surveyAttributeMapping.AttributeControlType == AttributeControlType.TextBox ||
                surveyAttributeMapping.AttributeControlType == AttributeControlType.MultilineTextbox ||
                surveyAttributeMapping.AttributeControlType == AttributeControlType.Datepicker ||
                surveyAttributeMapping.AttributeControlType == AttributeControlType.FileUpload)
                return true;

            //other attribute control types does not have validation
            return false;
        }

        public static bool IsNonCombinable(this SurveyAttributeMapping surveyAttributeMapping)
        {
            //When you have a survey with several attributes it may well be that some are combinable,
            //whose combination may form a new SKU with its own inventory,
            //and some non-combinable are more used to add accessories

            if (surveyAttributeMapping == null)
                return false;

            //we can add a new property to "SurveyAttributeMapping" entity indicating whether it's combinable/non-combinable
            //but we assume that attributes
            //which cannot have values (any value can be entered by a customer)
            //are non-combinable
            var result = !ShouldHaveValues(surveyAttributeMapping);
            return result;
        }
    }
}
