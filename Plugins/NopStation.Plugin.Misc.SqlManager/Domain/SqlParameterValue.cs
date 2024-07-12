using Nop.Core;

namespace NopStation.Plugin.Misc.SqlManager.Domain
{
    public class SqlParameterValue : BaseEntity
    {
        public int SqlParameterId { get; set; }

        public string Value { get; set; }
    }
}
