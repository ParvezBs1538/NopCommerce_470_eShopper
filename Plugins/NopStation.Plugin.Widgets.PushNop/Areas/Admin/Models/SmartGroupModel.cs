using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models
{
    public record SmartGroupModel : BaseNopEntityModel
    {
        public SmartGroupModel()
        {
            SmartGroupConditionSearchModel = new SmartGroupConditionSearchModel();
        }

        [NopResourceDisplayName("Admin.NopStation.PushNop.SmartGroups.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.SmartGroups.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        public SmartGroupConditionSearchModel SmartGroupConditionSearchModel { get; set; }
    }
}
