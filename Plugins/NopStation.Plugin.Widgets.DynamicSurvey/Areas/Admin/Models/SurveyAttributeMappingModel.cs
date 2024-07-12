using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public partial record SurveyAttributeMappingModel : BaseNopEntityModel, ILocalizedModel<SurveyAttributeMappingLocalizedModel>
    {
        #region Ctor

        public SurveyAttributeMappingModel()
        {
            AvailableSurveyAttributes = new List<SelectListItem>();
            Locales = new List<SurveyAttributeMappingLocalizedModel>();
            ConditionModel = new SurveyAttributeConditionModel();
            SurveyAttributeValueSearchModel = new SurveyAttributeValueSearchModel();
        }

        #endregion

        #region Properties

        public int SurveyId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.Attribute")]
        public int SurveyAttributeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.Attribute")]
        public string SurveyAttribute { get; set; }

        public IList<SelectListItem> AvailableSurveyAttributes { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.TextPrompt")]
        public string TextPrompt { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.IsRequired")]
        public bool IsRequired { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.AttributeControlType")]
        public int AttributeControlTypeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.AttributeControlType")]
        public string AttributeControlType { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        //validation fields
        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MinLength")]
        [UIHint("Int32Nullable")]
        public int? ValidationMinLength { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MaxLength")]
        [UIHint("Int32Nullable")]
        public int? ValidationMaxLength { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.DefaultDate")]
        [UIHint("DateNullable")]
        public DateTime? DefaultDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MinDate")]
        [UIHint("DateNullable")]
        public DateTime? ValidationMinDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MaxDate")]
        [UIHint("DateNullable")]
        public DateTime? ValidationMaxDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.FileAllowedExtensions")]
        public string ValidationFileAllowedExtensions { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.FileMaximumSize")]
        [UIHint("Int32Nullable")]
        public int? ValidationFileMaximumSize { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.DefaultValue")]
        public string DefaultValue { get; set; }

        public string ValidationRulesString { get; set; }

        //condition
        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Condition")]
        public bool ConditionAllowed { get; set; }

        public string ConditionString { get; set; }

        public SurveyAttributeConditionModel ConditionModel { get; set; }

        public IList<SurveyAttributeMappingLocalizedModel> Locales { get; set; }

        public SurveyAttributeValueSearchModel SurveyAttributeValueSearchModel { get; set; }

        #endregion
    }

    public partial record SurveyAttributeMappingLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.TextPrompt")]
        public string TextPrompt { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.DefaultValue")]
        public string DefaultValue { get; set; }
    }
}