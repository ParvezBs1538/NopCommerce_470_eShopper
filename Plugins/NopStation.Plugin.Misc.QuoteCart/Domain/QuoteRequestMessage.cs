using System;
using Nop.Core;

namespace NopStation.Plugin.Misc.QuoteCart.Domain;

public class QuoteRequestMessage : BaseEntity
{
    public int QuoteRequestId { get; set; }

    public int CustomerId { get; set; }

    public int StoreId { get; set; }

    public string Subject { get; set; }

    public string Content { get; set; }

    public DateTime CreatedOnUtc { get; set; }
}
