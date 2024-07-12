using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models
{
    public class SqlReportFilterOptionModel
    {
        public SqlReportFilterOptionModel()
        {
            AvailableValues = new List<SelectListItem>();
            SelectedValues = new List<string>();
        }

        public string Name { get; set; }

        public string SystemName { get; set; }

        public int Order { get; set; }

        public bool IsListItem { get; set; }

        public bool IsDateItem { get; set; }

        public bool IsTextInputItem { get; set; }

        public string FormattedValue { get; set; }

        public IList<string> SelectedValues { get; set; }

        public IList<SelectListItem> AvailableValues { get; set; }
    }
}