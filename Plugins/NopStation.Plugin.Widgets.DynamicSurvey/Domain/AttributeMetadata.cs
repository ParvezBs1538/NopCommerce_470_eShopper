using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Domain
{
    public class AttributeMetadata
    {
        public AttributeMetadata()
        {
            Mappings = new List<MappingData>();
        }

        public IList<MappingData> Mappings { get; set; }

        public class MappingData
        {
            public MappingData(SurveyAttributeMapping mapping, string value)
            {
                AttributeMapping = mapping;
                AttributeValue = value;
            }

            public SurveyAttributeMapping AttributeMapping { get; set; }

            public string AttributeValue { get; set; }
        }
    }
}
