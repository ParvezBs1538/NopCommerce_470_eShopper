using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Domains;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(ProductQnA), "NS_QnA_ProductQA" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {

        };
    }
}
