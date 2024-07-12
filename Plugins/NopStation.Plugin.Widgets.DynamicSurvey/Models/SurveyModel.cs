using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Models
{
    public record SurveyModel : BaseNopEntityModel
    {
        public SurveyModel()
        {
            SurveyAttributes = new List<SurveyAttributeModel>();
        }

        public string SeName { get; set; }

        public string SystemName { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public bool SuccessfullySent { get; set; }

        public string Result { get; set; }


        public bool DisplayCaptcha { get; set; }

        public IList<SurveyAttributeModel> SurveyAttributes { get; set; }


        public partial record SurveyAttributeModel : BaseNopEntityModel
        {
            public SurveyAttributeModel()
            {
                AllowedFileExtensions = new List<string>();
                Values = new List<SurveyAttributeValueModel>();
            }

            public int SurveyId { get; set; }

            public int SurveyAttributeId { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            public string DefaultValue { get; set; }
            public int? SelectedDay { get; set; }
            public int? SelectedMonth { get; set; }
            public int? SelectedYear { get; set; }

            public DateTime? ValidationMinDate { get; set; }

            public DateTime? ValidationMaxDate { get; set; }

            public bool HasCondition { get; set; }

            public IList<string> AllowedFileExtensions { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<SurveyAttributeValueModel> Values { get; set; }
        }

        public partial record SurveyAttributeValueModel : BaseNopEntityModel
        {
            public SurveyAttributeValueModel()
            {
                ImageSquaresPictureModel = new PictureModel();
            }

            public string Name { get; set; }

            public PictureModel ImageSquaresPictureModel { get; set; }

            public string ColorSquaresRgb { get; set; }

            public bool IsPreSelected { get; set; }
        }
    }
}
