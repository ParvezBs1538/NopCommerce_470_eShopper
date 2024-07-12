using System;
using Nop.Core;

namespace NopStation.Plugin.Misc.QuoteCart.Domain;

public class QuoteCartItem : BaseEntity
{
    public int StoreId { get; set; }

    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public string AttributesXml { get; set; }

    public decimal CustomerEnteredPrice { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    public DateTime? RentalStartDateUtc { get; set; }

    public DateTime? RentalEndDateUtc { get; set; }

}
