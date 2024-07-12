using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public partial record SurveyAttributeModel : BaseNopEntityModel, ILocalizedModel<SurveyAttributeLocalizedModel>
    {
        #region Ctor

        public SurveyAttributeModel()
        {
            Locales = new List<SurveyAttributeLocalizedModel>();
            PredefinedSurveyAttributeValueSearchModel = new PredefinedSurveyAttributeValueSearchModel();
            SurveyAttributeSurveySearchModel = new SurveyAttributeSurveySearchModel();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveyAttributes.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveyAttributes.Fields.Description")]
        public string Description { get; set; }

        public IList<SurveyAttributeLocalizedModel> Locales { get; set; }

        public PredefinedSurveyAttributeValueSearchModel PredefinedSurveyAttributeValueSearchModel { get; set; }

        public SurveyAttributeSurveySearchModel SurveyAttributeSurveySearchModel { get; set; }

        #endregion
    }

    public partial record SurveyAttributeLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveyAttributes.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveyAttributes.Fields.Description")]
        public string Description { get; set; }
    }
}