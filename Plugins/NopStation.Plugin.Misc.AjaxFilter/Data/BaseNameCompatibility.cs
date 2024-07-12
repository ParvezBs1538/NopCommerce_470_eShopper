using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {

            { typeof(AjaxFilterSpecificationAttribute), "NS_AjaxFilter_SpecificationAttribute" },
            { typeof(AjaxFilterParentCategory),"NS_AjaxFilter_ParentCategory" },

        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
