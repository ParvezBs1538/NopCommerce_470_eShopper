using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public partial record QuoteRequestModel : BaseNopEntityModel
{
    public string StoreName { get; set; }

    public int CustomerId { get; set; }

    public Guid RequestId { get; set; }

    public string CustomerInfo { get; set; }

    public string CustomerEmail { get; set; }

    public string CustomerFullName { get; set; }

    public string CustomerIp { get; set; }

    public Dictionary<string, object> CustomValues { get; set; }

    public string RequestStatus => ((RequestStatus)RequestStatusId).ToString();

    public int RequestStatusId { get; set; }

    public string RequestType { get; set; }

    public int RequestTypeId { get; set; }

    public DateTime CreatedOn { get; set; }

    public string ShareQuote { get; set; }
}
