using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.TinyPNG.Domain;

namespace NopStation.Plugin.Misc.TinyPNG.Data
{
    public partial class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
             { typeof(PictureInfo), "NS_TinyPNG_PictureInfo" },
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string> { };
    }
}