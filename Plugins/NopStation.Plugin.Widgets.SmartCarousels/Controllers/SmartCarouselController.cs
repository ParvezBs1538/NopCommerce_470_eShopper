using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.SmartCarousels.Factories;
using NopStation.Plugin.Widgets.SmartCarousels.Services;

namespace NopStation.Plugin.Widgets.SmartCarousels.Controllers;

public class SmartCarouselController : NopStationPublicController
{
    private readonly ISmartCarouselModelFactory _carouselModelFactory;
    private readonly ISmartCarouselService _carouselService;
    private readonly IAclService _aclService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IPermissionService _permissionService;

    public SmartCarouselController(ISmartCarouselModelFactory carouselModelFactory,
        ISmartCarouselService carouselService,
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
        var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels);
        if (notAvailable && !hasAdminAccess)
            return Json(new { result = false });

        var model = await _carouselModelFactory.PrepareCarouselModelAsync(carousel);

        string viewPath;
        switch (model.CarouselType)
        {
            case Domains.CarouselType.Product:
                viewPath = "ProductSmartCarousel";
                break;
            case Domains.CarouselType.Manufacturer:
                viewPath = "ManufacturerSmartCarousel";
                break;
            case Domains.CarouselType.Category:
                viewPath = "CategorySmartCarousel";
                break;
            case Domains.CarouselType.Vendor:
                viewPath = "VendorSmartCarousel";
                break;
            case Domains.CarouselType.Picture:
            default:
                viewPath = "PictureSmartCarousel";
                break;
        }

        var html = await RenderPartialViewToStringAsync(viewPath, model);

        return Json(new { result = true, html = html, carouselid = carouselId });
    }
}
