using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Models;

public record QuoteRequestItemModel : BaseNopEntityModel
{
    public int QuoteRequestId { get; set; }

    public int StoreId { get; set; }

    public string ProductName { get; set; }

    public string ProductSku { get; set; }

    public string ProductPrice { get; set; }

    public decimal ProductPriceValue { get; set; }

    public string ProductSeName { get; set; }

    public string DiscountedPrice { get; set; }

    public decimal DiscountedPriceValue { get; set; }

    public string ItemTotal { get; set; }

    public decimal ItemTotalValue { get; set; }

    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public string AttributesXml { get; set; }

    public int Quantity { get; set; }

    public string RentalInfo { get; set; }
}
