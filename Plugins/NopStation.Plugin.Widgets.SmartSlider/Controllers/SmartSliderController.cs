using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.SmartSliders.Factories;
using NopStation.Plugin.Widgets.SmartSliders.Services;

namespace NopStation.Plugin.Widgets.SmartSliders.Controllers;

public class SmartSliderController : NopStationPublicController
{
    private readonly ISmartSliderModelFactory _sliderModelFactory;
    private readonly ISmartSliderService _sliderService;
    private readonly IAclService _aclService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IPermissionService _permissionService;

    public SmartSliderController(ISmartSliderModelFactory sliderModelFactory,
        ISmartSliderService sliderService,
        IAclService aclService,
        IStoreMappingService storeMappingService,
        IPermissionService permissionService)
    {
        _sliderModelFactory = sliderModelFactory;
        _sliderService = sliderService;
        _aclService = aclService;
        _storeMappingService = storeMappingService;
        _permissionService = permissionService;
    }

    [HttpPost]
    public async Task<IActionResult> Details(int sliderId)
    {
        var slider = await _sliderService.GetSliderByIdAsync(sliderId);
        if (slider == null || !slider.Active)
            return Json(new { result = false });

        var notAvailable =
            //published?
            !slider.Active ||
            //ACL (access control list) 
            !await _aclService.AuthorizeAsync(slider) ||
            //Store mapping
            !await _storeMappingService.AuthorizeAsync(slider);
        //Check whether the current user has a "Manage products" permission (usually a store owner)
        //We should allows him (her) to use "Preview" functionality
        var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders);
        if (notAvailable && !hasAdminAccess)
            return Json(new { result = false });

        var model = await _sliderModelFactory.PrepareSliderModelAsync(slider);
        var html = await RenderPartialViewToStringAsync("SmartSlider", model);

        return Json(new { result = true, html = html, sliderid = sliderId });
    }
}

