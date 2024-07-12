using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

public partial record AddProductToConditionModel : BaseNopEntityModel
{
    #region Ctor

    public AddProductToConditionModel()
    {
        SelectedProductIds = new List<int>();
    }

    #endregion

    #region Properties

    public int EntityId { get; set; }

    public string EntityName { get; set; }

    public IList<int> SelectedProductIds { get; set; }

    #endregion
}