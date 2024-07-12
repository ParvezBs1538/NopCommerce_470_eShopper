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
    public class CountryBlockRuleController : NopStationAdminController
    {
        #region Fields

        private readonly ICountryBlockRuleService _countryBlockRuleService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ICountryBlockRuleModelFactory _countryBlockRuleModelFactory;

        #endregion

        #region Ctor

        public CountryBlockRuleController(ICountryBlockRuleService countryBlockRuleService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ICountryBlockRuleModelFactory countryBlockRuleModelFactory)
        {
            _countryBlockRuleService = countryBlockRuleService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _countryBlockRuleModelFactory = countryBlockRuleModelFactory;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _countryBlockRuleModelFactory.PrepareCountryBlockRuleSearchModelAsync(new CountryBlockRuleSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(CountryBlockRuleSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _countryBlockRuleModelFactory.PrepareCountryBlockRuleListModelAsync(searchModel); 

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _countryBlockRuleModelFactory.PrepareCountryBlockRuleModelAsync(new CountryBlockRuleModel(), null);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(CountryBlockRuleModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var countryBlockRule = model.ToEntity<CountryBlockRule>();
                countryBlockRule.CreatedOnUtc = DateTime.UtcNow;

                await _countryBlockRuleService.InsertCountryBlockRuleAsync(countryBlockRule);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.CountryBlockRules.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = countryBlockRule.Id });
            }

            model = await _countryBlockRuleModelFactory.PrepareCountryBlockRuleModelAsync(model, null);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var countryBlockRule = await _countryBlockRuleService.GetCountryBlockRuleByIdAsync(id);
            if (countryBlockRule == null)
                return RedirectToAction("List");

            var model = await _countryBlockRuleModelFactory.PrepareCountryBlockRuleModelAsync(null, countryBlockRule);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(CountryBlockRuleModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var countryBlockRule = await _countryBlockRuleService.GetCountryBlockRuleByIdAsync(model.Id);
            if (countryBlockRule == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                countryBlockRule = model.ToEntity(countryBlockRule);

                await _countryBlockRuleService.UpdateCountryBlockRuleAsync(countryBlockRule);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.CountryBlockRules.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = countryBlockRule.Id });
            }

            model = await _countryBlockRuleModelFactory.PrepareCountryBlockRuleModelAsync(null, countryBlockRule);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Delete(CountryBlockRuleModel model)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var countryBlockRule = await _countryBlockRuleService.GetCountryBlockRuleByIdAsync(model.Id);
            if (countryBlockRule == null)
                return RedirectToAction("List");

            await _countryBlockRuleService.DeleteCountryBlockRuleAsync(countryBlockRule);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.CountryBlockRules.Deleted"));

            return RedirectToAction("List");
        }

        #endregion
    }
}
