using System;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record QuoteRequestMessageModel : BaseNopEntityModel
{
    public int QuoteRequestId { get; set; }

    public bool IsWriter { get; set; }

    public int CustomerId { get; set; }

    public int StoreId { get; set; }

    public string Subject { get; set; }

    public string Content { get; set; }

    public DateTime CreatedOnUtc { get; set; }
}
