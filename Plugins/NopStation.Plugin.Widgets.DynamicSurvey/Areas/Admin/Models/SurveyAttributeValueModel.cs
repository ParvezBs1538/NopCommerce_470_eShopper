using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public partial record SurveyAttributeValueModel : BaseNopEntityModel, ILocalizedModel<SurveyAttributeValueLocalizedModel>
    {
        #region Ctor

        public SurveyAttributeValueModel()
        {
            Locales = new List<SurveyAttributeValueLocalizedModel>();
        }

        #endregion

        #region Properties

        public int SurveyAttributeMappingId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.ColorSquaresRgb")]
        public string ColorSquaresRgb { get; set; }

        public bool DisplayColorSquaresRgb { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.ImageSquaresPicture")]
        [UIHint("Picture")]
        public int ImageSquaresPictureId { get; set; }

        public bool DisplayImageSquaresPicture { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<SurveyAttributeValueLocalizedModel> Locales { get; set; }

        #endregion
    }

    public partial record SurveyAttributeValueLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.Name")]
        public string Name { get; set; }
    }
}