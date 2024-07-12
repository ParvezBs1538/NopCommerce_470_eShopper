using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nop.Core.Domain.Common;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Misc.Core.Controllers;

[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
[ValidateIpAddress]
[AuthorizeAdmin]
[ValidateVendor]
[SaveSelectedTab]
[NotNullValidationMessage]
[CheckAccess]
public class NopStationAdminController : BaseController
{
    public override JsonResult Json(object data)
    {
        //use IsoDateFormat on writing JSON text to fix issue with dates in grid
        var useIsoDateFormat = NopInstance.Load<AdminAreaSettings>()?.UseIsoDateFormatInJsonResult ?? false;
        var serializerSettings = NopInstance.Load<IOptions<MvcNewtonsoftJsonOptions>>()?.Value?.SerializerSettings
            ?? new JsonSerializerSettings();

        if (!useIsoDateFormat)
            return base.Json(data, serializerSettings);

        serializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        serializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;

        return base.Json(data, serializerSettings);
    }
}
