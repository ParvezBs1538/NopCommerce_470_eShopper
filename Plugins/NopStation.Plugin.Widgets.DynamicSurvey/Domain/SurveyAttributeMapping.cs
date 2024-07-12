using System;
using Nop.Core;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Domain
{
    public partial class SurveyAttributeMapping : BaseEntity, ILocalizedEntity
    {
        public int SurveyId { get; set; }

        public int SurveyAttributeId { get; set; }

        public string TextPrompt { get; set; }

        public bool IsRequired { get; set; }

        public int AttributeControlTypeId { get; set; }

        public int DisplayOrder { get; set; }

        public int? ValidationMinLength { get; set; }

        public int? ValidationMaxLength { get; set; }

        public DateTime? DefaultDate { get; set; }

        public DateTime? ValidationMinDate { get; set; }

        public DateTime? ValidationMaxDate { get; set; }

        public string ValidationFileAllowedExtensions { get; set; }

        public int? ValidationFileMaximumSize { get; set; }

        public string DefaultValue { get; set; }

        public string ConditionAttributeXml { get; set; }

        public AttributeControlType AttributeControlType
        {
            get => (AttributeControlType)AttributeControlTypeId;
            set => AttributeControlTypeId = (int)value;
        }
    }
}