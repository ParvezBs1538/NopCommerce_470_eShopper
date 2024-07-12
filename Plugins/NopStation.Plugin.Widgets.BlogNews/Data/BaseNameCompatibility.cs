using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widget.BlogNews.Domains;

namespace NopStation.Plugin.Widget.BlogNews.Data;

public class BaseNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
    {
        { typeof(BlogNewsPicture), "NS_BlogNewsPicture" },
    };

    public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
    {
    };
}