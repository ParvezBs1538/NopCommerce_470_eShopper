using System;
using System.Collections.Generic;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public partial record QuoteRequestDetailsModel : BaseNopEntityModel
{
    public QuoteRequestDetailsModel()
    {
        QuoteRequestItems = [];
        RequestMessages = [];
        SubmittedFormAttributes = [];
    }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.Id")]
    public override int Id { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.Customer")]
    public int CustomerId { get; set; }


    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.Store")]
    public string StoreName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.RequestGuid")]
    public string RequestGuid { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.RequestStatus")]
    public string RequestStatus { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.RequestStatus")]
    public int RequestStatusId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.RequestType")]
    public string RequestType { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.ShareQuote")]
    public string ShareQuote { get; set; }

    public string ResponseMessage { get; set; }

    public bool CanCancelRequest { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.Customer")]
    public CustomerModel CustomerModel { get; set; }

    public IList<QuoteRequestItemModel> QuoteRequestItems { get; set; }

    public IList<QuoteRequestMessageModel> RequestMessages { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.CreatedOnUtc")]
    public DateTime CreatedOnUtc { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.UpdatedOnUtc")]
    public DateTime UpdatedOnUtc { get; set; }

    public string SubTotalStr { get; set; }

    public string QuoteFormName { get; set; }

    public IList<SubmittedFormAttributeModel> SubmittedFormAttributes { get; set; }
}
