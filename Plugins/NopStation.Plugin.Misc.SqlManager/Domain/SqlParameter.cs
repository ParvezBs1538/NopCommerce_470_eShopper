using Nop.Core;

namespace NopStation.Plugin.Misc.SqlManager.Domain
{
    public class SqlParameter : BaseEntity
    {
        public string Name { get; set; }

        public string SystemName { get; set; }

        public int DataTypeId { get; set; }

        public DataType DataType
        {
            get => (DataType)DataTypeId;
            set => DataTypeId = (int)value;
        }
    }
}
