using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Models
{
    public partial record SliderSearchModel : BaseSearchModel
    {
        public SliderSearchModel()
        {
            AvailableSliderTypes = new List<SelectListItem>();
            SelectedSliderTypes = new List<int>();
        }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Sliders.List.SelectedSliderTypes")]
        public IList<int> SelectedSliderTypes { get; set; }

        public IList<SelectListItem> AvailableSliderTypes { get; set; }
    }
}
