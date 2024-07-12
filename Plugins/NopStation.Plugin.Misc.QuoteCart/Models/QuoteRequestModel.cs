using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Models;

public record QuoteRequestModel : BaseNopEntityModel
{
    public QuoteRequestModel()
    {
        QuoteRequestItems = [];
    }

    public string ProductsOverview { get; set; }

    public string QuoteRequestType { get; set; }

    public int QuoteRequestStatusId { get; set; }

    public string QuoteRequestStatus { get; set; }

    public DateTime CreatedOn { get; set; }

    public int FormId { get; set; }

    public Guid RequestId { get; set; }

    public string FormData { get; set; }

    public IList<QuoteRequestItemModel> QuoteRequestItems { get; set; }
}
