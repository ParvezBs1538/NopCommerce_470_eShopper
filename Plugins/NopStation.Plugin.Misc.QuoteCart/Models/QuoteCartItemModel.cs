using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.Misc.QuoteCart.Models;

public record QuoteCartItemModel : BaseNopEntityModel
{
    public string Sku { get; set; }

    public string VendorName { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; }

    public string ProductSeName { get; set; }

    public string AttributeInfo { get; set; }

    public string UnitPrice { get; set; }

    public decimal UnitPriceValue { get; set; }

    public string SubTotal { get; set; }

    public decimal SubTotalValue { get; set; }

    public int Quantity { get; set; }

    public string RentalInfo { get; set; }

    public PictureModel Picture { get; set; }
}
