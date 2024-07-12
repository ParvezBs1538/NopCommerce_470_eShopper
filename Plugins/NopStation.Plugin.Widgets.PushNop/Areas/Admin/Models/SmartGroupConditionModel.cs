using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models
{
    public record SmartGroupConditionModel : BaseNopEntityModel
    {
        public SmartGroupConditionModel()
        {
            AvailableLogicTypes = new List<SelectListItem>();
            AvailableConditionColumnTypes = new List<SelectListItem>();
            AvailableConditionTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.PushNop.SmartGroupConditions.Fields.Value")]
        public string ValueString { get; set; }
        public int ValueInt { get; set; }
        [UIHint("DateTimeNullable")]
        public DateTime? ValueDateTime { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.SmartGroupConditions.Fields.ConditionColumnType")]
        public int ConditionColumnTypeId { get; set; }
        public string ConditionColumnTypeStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.SmartGroupConditions.Fields.ConditionType")]
        public int ConditionTypeId { get; set; }
        public string ConditionTypeStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.SmartGroupConditions.Fields.LogicType")]
        public int LogicTypeId { get; set; }
        public string LogicTypeStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.SmartGroupConditions.Fields.SmartGroup")]
        public int SmartGroupId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.SmartGroupConditions.Fields.SmartGroup")]
        public string SmartGroupName { get; set; }

        public IList<SelectListItem> AvailableLogicTypes { get; set; }
        public IList<SelectListItem> AvailableConditionTypes { get; set; }
        public IList<SelectListItem> AvailableConditionColumnTypes { get; set; }
    }
}
