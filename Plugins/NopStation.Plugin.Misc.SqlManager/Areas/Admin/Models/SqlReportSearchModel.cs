using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models
{
    public record SqlReportSearchModel : BaseSearchModel
    {
        public SqlReportSearchModel()
        {
            SelectedCustomerRoleIds = new List<int>();
        }

        public IList<int> SelectedCustomerRoleIds { get; set; }
    }
}
