using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Models
{
    public record DeviceSearchModel : BaseSearchModel
    {
        public DeviceSearchModel()
        {
            SelectedDeviceTypes = new List<int>();
            AvailableDeviceTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Devices.List.SelectedDeviceTypes")]
        public IList<int> SelectedDeviceTypes { get; set; }

        public IList<SelectListItem> AvailableDeviceTypes { get; set; }
    }
}
