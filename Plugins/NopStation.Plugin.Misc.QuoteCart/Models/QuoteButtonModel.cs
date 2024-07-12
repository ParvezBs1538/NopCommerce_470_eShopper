using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Models;

public record QuoteButtonModel : BaseNopModel
{
    public QuoteButtonModel()
    {
        AllowedQuantities = [];
    }

    public int ProductId { get; set; }

    public bool AddToCartButtonEnabled { get; set; }

    public bool IsProductDetails { get; set; }

    public IList<SelectListItem> AllowedQuantities { get; set; }

    public int EnteredQuantity { get; set; }
}
