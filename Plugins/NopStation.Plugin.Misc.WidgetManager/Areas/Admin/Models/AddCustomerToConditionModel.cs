using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

public partial record AddCustomerToConditionModel : BaseNopEntityModel
{
    #region Ctor

    public AddCustomerToConditionModel()
    {
        SelectedCustomerIds = new List<int>();
    }

    #endregion

    #region Properties

    public int EntityId { get; set; }

    public string EntityName { get; set; }

    public IList<int> SelectedCustomerIds { get; set; }

    #endregion
}