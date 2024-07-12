using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.Flipbooks.Domains;
using NopStation.Plugin.Widgets.Flipbooks.Factories;
using NopStation.Plugin.Widgets.Flipbooks.Services;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace NopStation.Plugin.Widgets.Flipbooks.Controllers
{
    public class FlipbookController : NopStationPublicController
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IFlipbookModelFactory _flipbookFactory;
        private readonly IFlipbookService _flipbookService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public FlipbookController(ILogger logger,
            IFlipbookModelFactory flipbookFactory,
            IFlipbookService flipbookService,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            IPermissionService permissionService)
        {
            _logger = logger;
            _flipbookFactory = flipbookFactory;
            _flipbookService = flipbookService;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _permissionService = permissionService;
        }

        #endregion

        #region Utilities

        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task<bool> CheckFlipbookAvailabilityAsync(Flipbook flipbook)
        {
            var isAvailable = true;

            if (flipbook == null || flipbook.Deleted)
                isAvailable = false;

            var notAvailable =
                //published?
                !flipbook.Active ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(flipbook) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(flipbook) ||
                (flipbook.AvailableEndDateTimeUtc.HasValue && flipbook.AvailableEndDateTimeUtc.Value < DateTime.UtcNow) ||
                (flipbook.AvailableStartDateTimeUtc.HasValue && flipbook.AvailableStartDateTimeUtc.Value > DateTime.UtcNow);

            //Check whether the current user has a "Manage flipbooks" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks);
            if (notAvailable && !hasAdminAccess)
                isAvailable = false;

            return isAvailable;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Details(int flipbookid)
        {
            var flipbook = await _flipbookService.GetFlipbookByIdAsync(flipbookid);
            if (!await CheckFlipbookAvailabilityAsync(flipbook))
                return InvokeHttp404();

            var model = await _flipbookFactory.PrepareFlipbookDetailsModelAsync(flipbook);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LoadProductsByFlipbookContentId(int contentCatalogId, int pageNumber)
        {
            var success = false;
            var htmlResult = string.Empty;

            try
            {
                var pageIndex = pageNumber > 0 ? pageNumber - 1 : 0;
                var model = await _flipbookFactory.PrepareFlipbookContentProductsAsync(contentCatalogId, pageIndex);
                success = true;
                htmlResult = await RenderPartialViewToStringAsync("_CatalogProducts", model);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }

            return Json(new
            {
                success,
                htmlResult
            });
        }

        #endregion
    }
}
