using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerToMap
{
    public partial record ManufacturerToMapSearchModel : BaseSearchModel
    {
        public ManufacturerToMapSearchModel()
        {
            SelectedManufacturerIds = new List<int>();
        }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerToMapSearch.Fields.ManufacturerName")]
        public string ManufacturerName { get; set; }

        public int ManufacturerSEOTemplateId { get; set; }

        public IList<int> SelectedManufacturerIds { get; set; }
    }
}
