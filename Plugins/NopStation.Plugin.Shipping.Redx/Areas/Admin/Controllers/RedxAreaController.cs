using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Factories;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Models;
using NopStation.Plugin.Shipping.Redx.Domains;
using NopStation.Plugin.Shipping.Redx.Services;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Shipping.Redx.Models;
using System.Linq;
using Nop.Services.Messages;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Controllers
{
    public partial class RedxAreaController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly RedxSettings _redxSettings;
        private readonly IRedxAreaService _redxAreaService;
        private readonly IRedxAreaModelFactory _redxAreaModelFactory;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public RedxAreaController(IPermissionService permissionService,
            RedxSettings redxSettings,
            IRedxAreaService redxAreaService,
            IRedxAreaModelFactory redxAreaModelFactory,
            INotificationService notificationService,
            ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _redxSettings = redxSettings;
            _redxAreaService = redxAreaService;
            _redxAreaModelFactory = redxAreaModelFactory;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        [EditAccess, HttpPost, ActionName("List")]
        [FormValueRequired("sync-redx-area-list")]
        public async Task<IActionResult> SyncRedxArea()
        {
	        if (!await _permissionService.AuthorizeAsync(RedxShipmentPermissionProvider.ManageConfiguration))
		        return AccessDeniedView();

            var headers = new Dictionary<string, string>();
            headers.Add("API-ACCESS-TOKEN", "Bearer " + _redxSettings.ApiAccessToken);

            var model = _redxSettings.GetBaseUri().Concat("areas").Get<AreaResponseModel>(headers);
           
            if (model.Success)
            {
                var existingAreas = await _redxAreaService.GetRedxAreasAsync();

                for (var i = 0; i < existingAreas.Count; i++)
                {
                    var redxArea = existingAreas[i];
                    if (model.Model.Areas.Any(x => x.RedxAreaId == redxArea.RedxAreaId))
                        continue;

                    await _redxAreaService.DeleteRedxAreaAsync(redxArea);
                }

                foreach (var area in model.Model.Areas)
                {
                    if (await _redxAreaService.GetRedxAreaByRedxAreaIdAsync(area.RedxAreaId) is RedxArea redxArea)
                    {
                        redxArea.Name = area.Name;
                        redxArea.DistrictName = area.DistrictName;
                        redxArea.PostCode = area.PostCode;
                        redxArea.Deleted = false;

                        await _redxAreaService.UpdateRedxAreaAsync(redxArea);
                    }
                    else
                    {
                        var newRedxArea = new RedxArea
                        {
                            RedxAreaId = area.RedxAreaId,
                            Name = area.Name,
                            DistrictName = area.DistrictName,
                            PostCode = area.PostCode
                        };

                        await _redxAreaService.InsertRedxAreaAsync(newRedxArea);
                    }
                }
            }

            return RedirectToAction("List");
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(RedxShipmentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _redxAreaModelFactory.PrepareRedxAreaSearchModelAsync(new RedxAreaSearchModel());
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(RedxAreaSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(RedxShipmentPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var model = await _redxAreaModelFactory.PrepareRedxAreaListModelAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(RedxShipmentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var redxArea = await _redxAreaService.GetRedxAreaByRedxAreaIdAsync(id);
            if (redxArea == null || redxArea.Deleted)
                return RedirectToAction("List");

            var model = await _redxAreaModelFactory.PrepareRedxAreaModelAsync(null, redxArea);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(RedxAreaModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(RedxShipmentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var redxArea = await _redxAreaService.GetRedxAreaByRedxAreaIdAsync(model.RedxAreaId);
            if (redxArea == null || redxArea.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                redxArea = model.ToEntity(redxArea);
                await _redxAreaService.UpdateRedxAreaAsync(redxArea);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Redx.RedxArea.Updated"));

                return continueEditing ? RedirectToAction("Edit", new { id = redxArea.Id }) :
                    RedirectToAction("List");
            }

            model = await _redxAreaModelFactory.PrepareRedxAreaModelAsync(model, redxArea);
            return View(model);
        }

        #endregion
    }
}