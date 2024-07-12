using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;

namespace NopStation.Plugin.Misc.Core.Controllers;

public class NopStationController : BaseAdminController
{
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;

    public NopStationController(ILocalizationService localizationService,
        INotificationService notificationService)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
    }

    public IActionResult EditAccessRedirect(string returnUrl)
    {
        _notificationService.WarningNotification(_localizationService.GetResourceAsync("Admin.NopStation.Core.Resources.EditAccessDenied").Result);

        if (!string.IsNullOrWhiteSpace(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home", new { area = "Admin" });
    }
}
