using System;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Models;

public record QuoteRequestMessageModel : BaseNopEntityModel
{
    public int QuoteRequestId { get; set; }

    public int CustomerId { get; set; }

    public int StoreId { get; set; }

    public string Subject { get; set; }

    public string Content { get; set; }

    public DateTime SentOn { get; set; }

    public bool IsWriter { get; set; }

    public string CustomerEmail { get; set; }

    public string CustomerName { get; set; }
}
