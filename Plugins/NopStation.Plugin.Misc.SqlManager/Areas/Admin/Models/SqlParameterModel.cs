using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models
{
    public record SqlParameterModel : BaseNopEntityModel
    {
        public SqlParameterModel()
        {
            AvailableDataTypes = new List<SelectListItem>();
            SqlParameterValueSearchModel = new SqlParameterValueSearchModel();
            AddSqlParameterValueModel = new SqlParameterValueModel();
        }

        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlParameters.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlParameters.Fields.SystemName")]
        public string SystemName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlParameters.Fields.DataType")]
        public int DataTypeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlParameters.Fields.DataType")]
        public string DataTypeStr { get; set; }

        public bool IsDateType { get; set; }

        public bool IsTextInputItem { get; set; }

        public SqlParameterValueSearchModel SqlParameterValueSearchModel { get; set; }

        public SqlParameterValueModel AddSqlParameterValueModel { get; set; }

        public IList<SelectListItem> AvailableDataTypes { get; set; }
    }
}
