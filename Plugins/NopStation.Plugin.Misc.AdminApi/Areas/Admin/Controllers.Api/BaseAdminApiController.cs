using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.AdminApi.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[TokenAuthorize]
[PublishModelEvents]
[NstAuthorize]
[ValidateIpAddress]
[AuthorizeAdmin]
[ValidateVendor]
[NotNullValidationMessage]
public abstract partial class BaseAdminApiController : NopStationApiController
{
    protected virtual IActionResult AdminApiAccessDenied()
    {
        return Unauthorized(NopInstance.Load<ILocalizationService>()
            .GetResourceAsync("Admin.AccessDenied.Description")
            .GetAwaiter()
            .GetResult());
    }
}