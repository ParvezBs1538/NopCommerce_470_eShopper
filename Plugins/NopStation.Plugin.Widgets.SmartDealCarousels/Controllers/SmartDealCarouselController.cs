using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.SmartDealCarousels.Factories;
using NopStation.Plugin.Widgets.SmartDealCarousels.Services;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Controllers;

public class SmartDealCarouselController : NopStationPublicController
{
    private readonly ISmartDealCarouselModelFactory _carouselModelFactory;
    private readonly ISmartDealCarouselService _carouselService;
    private readonly IAclService _aclService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IPermissionService _permissionService;

    public SmartDealCarouselController(ISmartDealCarouselModelFactory carouselModelFactory,
        ISmartDealCarouselService carouselService,
        IAclService aclService,
        IStoreMappingService storeMappingService,
        IPermissionService permissionService)
    {
        _carouselModelFactory = carouselModelFactory;
        _carouselService = carouselService;
        _aclService = aclService;
        _storeMappingService = storeMappingService;
        _permissionService = permissionService;
    }

    [HttpPost]
    public async Task<IActionResult> Details(int carouselId)
    {
        var carousel = await _carouselService.GetCarouselByIdAsync(carouselId);
        if (carousel == null || !carousel.Active)
            return Json(new { result = false });

        var notAvailable =
            //published?
            !carousel.Active ||
            //ACL (access control list) 
            !await _aclService.AuthorizeAsync(carousel) ||
            //Store mapping
            !await _storeMappingService.AuthorizeAsync(carousel);
        //Check whether the current user has a "Manage products" permission (usually a store owner)
        //We should allows him (her) to use "Preview" functionality
        var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels);
        if (notAvailable && !hasAdminAccess)
            return Json(new { result = false });

        var model = await _carouselModelFactory.PrepareCarouselModelAsync(carousel);

        var html = await RenderPartialViewToStringAsync("SmartDealCarousel", model);

        return Json(new { result = true, html = html, carouselid = carouselId });
    }
}
