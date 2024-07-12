using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Models;

public record QuoteRequestDetailsModel : BaseNopEntityModel
{
    public QuoteRequestDetailsModel()
    {
        QuoteRequestItems = [];
        RequestMessages = [];
        SubmittedFormAttributes = [];
    }

    public int CustomerId { get; set; }

    public string RequestType { get; set; }

    public Guid RequestId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public int RequestStatusId { get; set; }

    public int StoreId { get; set; }

    public string Store { get; set; }

    public string RequestStatus { get; set; }

    public string ResponseMessage { get; set; }

    public string RequestTotals { get; set; }

    public IList<QuoteRequestItemModel> QuoteRequestItems { get; set; }

    public IList<QuoteRequestMessageModel> RequestMessages { get; set; }

    public IList<SubmittedFormAttributeModel> SubmittedFormAttributes { get; set; }

    public string GuestEmail { get; set; }
}
