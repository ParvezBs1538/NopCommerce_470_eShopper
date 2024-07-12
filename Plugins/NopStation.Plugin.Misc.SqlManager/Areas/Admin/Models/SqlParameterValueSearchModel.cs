using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models
{
    public record SqlParameterValueSearchModel : BaseSearchModel
    {
        public int SqlParameterId { get; set; }
    }
}
