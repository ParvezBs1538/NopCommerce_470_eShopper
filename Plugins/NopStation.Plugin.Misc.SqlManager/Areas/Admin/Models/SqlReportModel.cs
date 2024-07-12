using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models
{
    public record SqlReportModel : BaseNopEntityModel, IAclSupportedModel
    {
        public SqlReportModel()
        {
            AvailableParameters = new List<string>();
            AvailableCustomerRoles = new List<SelectListItem>();
            SelectedCustomerRoleIds = new List<int>();
            SqlReportFilterOptions = new List<SqlReportFilterOptionModel>();
        }

        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlReports.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlReports.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlReports.Fields.Query")]
        public string Query { get; set; }

        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlReports.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlReports.Fields.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlReports.Fields.AvailableParameters")]
        public IList<string> AvailableParameters { get; set; }

        public IList<SqlReportFilterOptionModel> SqlReportFilterOptions { get; set; }

        //ACL (customer roles)
        [NopResourceDisplayName("Admin.NopStation.SqlManager.SqlReports.Fields.AclCustomerRoles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
    }
}
