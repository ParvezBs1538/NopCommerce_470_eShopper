using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Data
{
    public partial class NameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(FAQCategory), "NS_FAQ_Category" },
            { typeof(FAQItem), "NS_FAQ_Item" },
            { typeof(FAQItemCategory), "NS_FAQ_Item_Category_Mapping" },
            { typeof(FAQItemTag), "NS_FAQ_Item_Tag_Mapping" },
            { typeof(FAQTag), "NS_FAQ_Tag" },
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>();
    }
}
