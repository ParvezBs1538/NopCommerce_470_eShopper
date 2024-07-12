using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models
{
    public record SqlParameterValueModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlParameterValues.Fields.SqlParameter")]
        public string SqlParameterName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlParameterValues.Fields.SqlParameter")]
        public int SqlParameterId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlParameterValues.Fields.Value")]
        public string Value { get; set; }

        public bool IsValid { get; set; }
    }
}
