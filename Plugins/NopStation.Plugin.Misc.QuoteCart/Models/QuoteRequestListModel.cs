using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Infrastructure;
using Nop.Web.Models.Common;

namespace NopStation.Plugin.Misc.QuoteCart.Models;

public record QuoteRequestListModel : BaseNopEntityModel
{
    public QuoteRequestListModel()
    {
        QuoteRequests = [];
    }
    public List<QuoteRequestModel> QuoteRequests { get; set; }

    public PagerModel PagerModel { get; set; }

    public class QuoteRequestRouteValues : IRouteValues
    {
        public int PageNumber { get; set; }
    }
}
