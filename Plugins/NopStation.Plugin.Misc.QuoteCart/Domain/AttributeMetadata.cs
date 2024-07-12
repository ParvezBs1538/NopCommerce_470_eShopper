using System.Collections.Generic;

namespace NopStation.Plugin.Misc.QuoteCart.Domain;

public class AttributeMetadata
{
    public AttributeMetadata()
    {
        Mappings = [];
    }

    public IList<MappingData> Mappings { get; set; }

    public class MappingData
    {
        public MappingData(FormAttributeMapping mapping, string value)
        {
            AttributeMapping = mapping;
            AttributeValue = value;
        }

        public FormAttributeMapping AttributeMapping { get; set; }

        public string AttributeValue { get; set; }
    }
}
