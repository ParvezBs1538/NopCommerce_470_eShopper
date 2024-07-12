using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.WebApi.Filters;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [TokenAuthorize]
    [PublishModelEvents]
    [DeviceIdAuthorize]
    [SaveIpAddress]
    [SaveLastActivity]
    [NstAuthorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BaseApiController : NopStationApiController
    {
    }
}
