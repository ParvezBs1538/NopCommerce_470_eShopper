using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Core.Models;

public record ConfigurationModel : BaseNopModel, ISettingsModel
{
    public ConfigurationModel()
    {
        AllowedCustomerRoleIds = new List<int>();
        AvailableCustomerRoles = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Admin.NopStation.Core.Configuration.Fields.RestrictMainMenuByCustomerRoles")]
    public bool RestrictMainMenuByCustomerRoles { get; set; }
    public bool RestrictMainMenuByCustomerRoles_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Core.Configuration.Fields.AllowedCustomerRoles")]
    public IList<int> AllowedCustomerRoleIds { get; set; }
    public bool AllowedCustomerRoleIds_OverrideForStore { get; set; }

    public IList<SelectListItem> AvailableCustomerRoles { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}
