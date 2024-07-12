using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public partial record SurveyAttributeConditionModel : BaseNopModel
    {
        public SurveyAttributeConditionModel()
        {
            SurveyAttributes = new List<SurveyAttributeModel>();
        }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Condition.EnableCondition")]
        public bool EnableCondition { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Condition.Attributes")]
        public int SelectedSurveyAttributeId { get; set; }
        public IList<SurveyAttributeModel> SurveyAttributes { get; set; }

        public int SurveyAttributeMappingId { get; set; }

        #region Nested classes

        public partial record SurveyAttributeModel : BaseNopEntityModel
        {
            public SurveyAttributeModel()
            {
                Values = new List<SurveyAttributeValueModel>();
            }

            public int SurveyAttributeId { get; set; }

            public string Name { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<SurveyAttributeValueModel> Values { get; set; }
        }

        public partial record SurveyAttributeValueModel : BaseNopEntityModel
        {
            public string Name { get; set; }

            public bool IsPreSelected { get; set; }
        }

        #endregion
    }
}