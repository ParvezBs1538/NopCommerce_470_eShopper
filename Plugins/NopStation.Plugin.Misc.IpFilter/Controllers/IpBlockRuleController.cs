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
    public class IpBlockRuleController : NopStationAdminController
    {
        #region Fields

        private readonly IIpBlockRuleService _ipBlockRuleService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IIpBlockRuleModelFactory _ipBlockRuleModelFactory;

        #endregion

        #region Ctor

        public IpBlockRuleController(IIpBlockRuleService ipBlockRuleService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IIpBlockRuleModelFactory ipBlockRuleModelFactory)
        {
            _ipBlockRuleService = ipBlockRuleService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _ipBlockRuleModelFactory = ipBlockRuleModelFactory;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _ipBlockRuleModelFactory.PrepareIpBlockRuleSearchModelAsync(new IpBlockRuleSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(IpBlockRuleSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _ipBlockRuleModelFactory.PrepareIpBlockRuleListModelAsync(searchModel); 

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _ipBlockRuleModelFactory.PrepareIpBlockRuleModelAsync(new IpBlockRuleModel(), null);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(IpBlockRuleModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var ipBlockRule = model.ToEntity<IpBlockRule>();
                ipBlockRule.CreatedOnUtc = DateTime.UtcNow;

                await _ipBlockRuleService.InsertIpBlockRuleAsync(ipBlockRule);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.IpBlockRules.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = ipBlockRule.Id });
            }

            model = await _ipBlockRuleModelFactory.PrepareIpBlockRuleModelAsync(model, null);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var ipBlockRule = await _ipBlockRuleService.GetIpBlockRuleByIdAsync(id);
            if (ipBlockRule == null)
                return RedirectToAction("List");

            var model = await _ipBlockRuleModelFactory.PrepareIpBlockRuleModelAsync(null, ipBlockRule);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(IpBlockRuleModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var ipBlockRule = await _ipBlockRuleService.GetIpBlockRuleByIdAsync(model.Id);
            if (ipBlockRule == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                ipBlockRule = model.ToEntity(ipBlockRule);

                await _ipBlockRuleService.UpdateIpBlockRuleAsync(ipBlockRule);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.IpBlockRules.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = ipBlockRule.Id });
            }

            model = await _ipBlockRuleModelFactory.PrepareIpBlockRuleModelAsync(null, ipBlockRule);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Delete(IpBlockRuleModel model)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var ipBlockRule = await _ipBlockRuleService.GetIpBlockRuleByIdAsync(model.Id);
            if (ipBlockRule == null)
                return RedirectToAction("List");

            await _ipBlockRuleService.DeleteIpBlockRuleAsync(ipBlockRule);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.IpBlockRules.Deleted"));

            return RedirectToAction("List");
        }

        #endregion
    }
}
