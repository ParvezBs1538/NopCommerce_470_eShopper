using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.PushNop.Domains
{
    public class SmartGroupCondition : BaseEntity
    {
        public string ValueString { get; set; }

        public int ValueInt { get; set; }

        public DateTime? ValueDateTime { get; set; }

        public int ConditionColumnTypeId { get; set; }

        public int ConditionTypeId { get; set; }

        public int LogicTypeId { get; set; }

        public int SmartGroupId { get; set; }

        public ConditionColumnType ConditionColumnType
        {
            get => (ConditionColumnType)ConditionColumnTypeId;
            set => ConditionColumnTypeId = (int)value;
        }

        public ConditionType ConditionType
        {
            get => (ConditionType)ConditionTypeId;
            set => ConditionTypeId = (int)value;
        }

        public LogicType LogicType
        {
            get => (LogicType)LogicTypeId;
            set => LogicTypeId = (int)value;
        }
    }
}
