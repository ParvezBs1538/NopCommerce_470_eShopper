using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Models;

public record CartModel : BaseNopModel
{
    public CartModel()
    {
        Items = [];
        Forms = [];
    }

    public bool CustomerCanEnterPrice { get; set; }

    public IList<QuoteCartItemModel> Items { get; set; }

    public IList<QuoteFormModel> Forms { get; set; }
}
