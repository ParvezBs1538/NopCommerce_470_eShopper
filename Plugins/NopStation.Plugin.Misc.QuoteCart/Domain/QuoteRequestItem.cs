using System;
using Nop.Core;

namespace NopStation.Plugin.Misc.QuoteCart.Domain;

public class QuoteRequestItem : BaseEntity
{
    public int QuoteRequestId { get; set; }

    public int StoreId { get; set; }

    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public string AttributesXml { get; set; }

    public int Quantity { get; set; }

    public decimal DiscountedPrice { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    public DateTime? RentalStartDateUtc { get; set; }

    public DateTime? RentalEndDateUtc { get; set; }
}
