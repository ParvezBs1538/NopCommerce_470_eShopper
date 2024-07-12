using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.DMS.Filters;

namespace NopStation.Plugin.Widgets.DMS.Controllers.API
{
    [TokenAuthorize]
    [SaveIpAddress]
    [SaveLastActivity]
    [NstAuthorize]
    [EnsureDeviceId]
    public class BaseApiController : NopStationApiController
    {
    }
}
