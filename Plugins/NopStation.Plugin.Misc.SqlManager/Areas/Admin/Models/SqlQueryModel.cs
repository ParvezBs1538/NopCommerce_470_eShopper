using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models
{
    public record SqlQueryModel : BaseNopEntityModel
    {
        public SqlQueryModel()
        {
            Results = new List<Dictionary<string, object>>();
        }

        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlReports.Fields.Query")]
        public string SQLQuery { get; set; }

        public IEnumerable<Dictionary<string, object>> Results { get; set; }

        public string Message { get; set; }

        public int ReturnedRow { get; set; }
    }
}
