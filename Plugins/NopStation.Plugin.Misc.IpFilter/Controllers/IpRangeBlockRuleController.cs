using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.IpFilter.Domain;
using NopStation.Plugin.Misc.IpFilter.Factories;
using NopStation.Plugin.Misc.IpFilter.Models;
using NopStation.Plugin.Misc.IpFilter.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace NopStation.Plugin.Misc.IpFilter.Controllers
{
    public class IpRangeBlockRuleController : NopStationAdminController
    {
        #region Fields

        private readonly IIpRangeBlockRuleService _ipRangeBlockRuleService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IIpRangeBlockRuleModelFactory _ipRangeBlockRuleModelFactory;

        #endregion

        #region Ctor

        public IpRangeBlockRuleController(IIpRangeBlockRuleService ipRangeBlockRuleService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IIpRangeBlockRuleModelFactory ipRangeBlockRuleModelFactory)
        {
            _ipRangeBlockRuleService = ipRangeBlockRuleService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _ipRangeBlockRuleModelFactory = ipRangeBlockRuleModelFactory;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _ipRangeBlockRuleModelFactory.PrepareIpRangeBlockRuleSearchModelAsync(new IpRangeBlockRuleSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(IpRangeBlockRuleSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _ipRangeBlockRuleModelFactory.PrepareIpRangeBlockRuleListModelAsync(searchModel); 

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _ipRangeBlockRuleModelFactory.PrepareIpRangeBlockRuleModelAsync(new IpRangeBlockRuleModel(), null);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(IpRangeBlockRuleModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var ipRangeBlockRule = model.ToEntity<IpRangeBlockRule>();
                ipRangeBlockRule.CreatedOnUtc = DateTime.UtcNow;

                await _ipRangeBlockRuleService.InsertIpRangeBlockRuleAsync(ipRangeBlockRule);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.IpRangeBlockRules.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = ipRangeBlockRule.Id });
            }

            model = await _ipRangeBlockRuleModelFactory.PrepareIpRangeBlockRuleModelAsync(model, null);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var ipRangeBlockRule = await _ipRangeBlockRuleService.GetIpRangeBlockRuleByIdAsync(id);
            if (ipRangeBlockRule == null)
                return RedirectToAction("List");

            var model = await _ipRangeBlockRuleModelFactory.PrepareIpRangeBlockRuleModelAsync(null, ipRangeBlockRule);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(IpRangeBlockRuleModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var ipRangeBlockRule = await _ipRangeBlockRuleService.GetIpRangeBlockRuleByIdAsync(model.Id);
            if (ipRangeBlockRule == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                ipRangeBlockRule = model.ToEntity(ipRangeBlockRule);

                await _ipRangeBlockRuleService.UpdateIpRangeBlockRuleAsync(ipRangeBlockRule);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.IpRangeBlockRules.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = ipRangeBlockRule.Id });
            }

            model = await _ipRangeBlockRuleModelFactory.PrepareIpRangeBlockRuleModelAsync(null, ipRangeBlockRule);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Delete(IpRangeBlockRuleModel model)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var ipRangeBlockRule = await _ipRangeBlockRuleService.GetIpRangeBlockRuleByIdAsync(model.Id);
            if (ipRangeBlockRule == null)
                return RedirectToAction("List");

            await _ipRangeBlockRuleService.DeleteIpRangeBlockRuleAsync(ipRangeBlockRule);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.IpRangeBlockRules.Deleted"));

            return RedirectToAction("List");
        }

        #endregion
    }
}
