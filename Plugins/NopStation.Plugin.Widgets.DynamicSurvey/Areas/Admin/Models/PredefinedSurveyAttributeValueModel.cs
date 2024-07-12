using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public partial record PredefinedSurveyAttributeValueModel : BaseNopEntityModel, ILocalizedModel<PredefinedSurveyAttributeValueLocalizedModel>
    {
        #region Ctor

        public PredefinedSurveyAttributeValueModel()
        {
            Locales = new List<PredefinedSurveyAttributeValueLocalizedModel>();
        }

        #endregion

        #region Properties

        public int SurveyAttributeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<PredefinedSurveyAttributeValueLocalizedModel> Locales { get; set; }

        #endregion
    }

    public partial record PredefinedSurveyAttributeValueLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.Fields.Name")]
        public string Name { get; set; }
    }
}