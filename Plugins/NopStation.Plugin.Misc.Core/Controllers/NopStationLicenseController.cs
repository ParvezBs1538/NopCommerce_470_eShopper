using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Domains;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Models;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class NopStationLicenseController : BasePluginController
{
    private readonly ILicenseService _licenseService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;

    public NopStationLicenseController(ILicenseService licenseService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IPermissionService permissionService)
    {
        _licenseService = licenseService;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _permissionService = permissionService;
    }

    public async Task<IActionResult> License()
    {
        if (!await _permissionService.AuthorizeAsync(CorePermissionProvider.ManageLicense))
            return AccessDeniedView();

        return View(new LicenseModel());
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> License(LicenseModel model)
    {
        if (!await _permissionService.AuthorizeAsync(CorePermissionProvider.ManageLicense))
            return AccessDeniedView();

        var result = _licenseService.VerifyProductKey(model.LicenseString);

        switch (result)
        {
            case KeyVerificationResult.InvalidProductKey:
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.InvalidProductKey"));
                return View(model);
            case KeyVerificationResult.InvalidProduct:
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.InvalidProduct"));
                return View(model);
            case KeyVerificationResult.InvalidForDomain:
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.InvalidForDomain"));
                return View(model);
            case KeyVerificationResult.InvalidForNOPVersion:
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.InvalidForNOPVersion"));
                return View(model);
            case KeyVerificationResult.Valid:
                await _licenseService.InsertLicenseAsync(new License()
                {
                    Key = model.LicenseString
                });
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.Saved"));

                return RedirectToAction("License");
            default:
                return RedirectToAction("License");
        }
    }
}
