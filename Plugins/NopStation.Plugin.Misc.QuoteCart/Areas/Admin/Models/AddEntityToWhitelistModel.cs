using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record AddEntityToWhitelistModel : BaseNopModel
{
    public AddEntityToWhitelistModel()
    {
        SelectedEntityIds = [];
    }

    public IList<int> SelectedEntityIds { get; set; }
}
