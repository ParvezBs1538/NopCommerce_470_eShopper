using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Factories
{
    public interface IAjaxFilterBaseAdminModelFactory
    {
        Task PrepareDefaultItemAsync(IList<SelectListItem> items, bool withSpecialDefaultItem, string defaultItemText = null, string defaultItemValue = "0");
        Task PrepareSpecificationAttributeAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);
    }
}
