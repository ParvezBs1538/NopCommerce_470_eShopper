using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record QuoteFormSearchModel : BaseSearchModel
{
    public QuoteFormSearchModel()
    {
        AvailableStores = [];
        AvailableActiveOptions = [];
    }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SearchStore")]
    public int SearchStoreId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SearchActive")]
    public int SearchActiveId { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }
    public IList<SelectListItem> AvailableActiveOptions { get; set; }
}
