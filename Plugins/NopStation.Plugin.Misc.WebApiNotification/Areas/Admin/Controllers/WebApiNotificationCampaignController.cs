using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApiNotification.Domains;
using NopStation.Plugin.Misc.WebApiNotification.Services;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Controllers
{
    public class WebApiNotificationCampaignController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPushNotificationCampaignModelFactory _pushNotificationCampaignModelFactory;
        private readonly IPushNotificationCampaignService _campaignService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;

        #endregion

        #region Ctor

        public WebApiNotificationCampaignController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPushNotificationCampaignModelFactory pushNotificationCampaignModelFactory,
            IPushNotificationCampaignService campaignService,
            IDateTimeHelper dateTimeHelper,
            IPermissionService permissionService,
            ILocalizedEntityService localizedEntityService,
            ILanguageService languageService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _pushNotificationCampaignModelFactory = pushNotificationCampaignModelFactory;
            _campaignService = campaignService;
            _dateTimeHelper = dateTimeHelper;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
        }

        #endregion

        #region Utilities

        protected virtual async Task CopyLocalizationDataAsync(WebApiNotificationCampaign campaign, WebApiNotificationCampaign campaignCopy)
        {
            var languages = await _languageService.GetAllLanguagesAsync(true);

            //localization
            foreach (var lang in languages)
            {
                var name = await _localizationService.GetLocalizedAsync(campaign, x => x.Title, lang.Id, false, false);
                if (!string.IsNullOrEmpty(name))
                    await _localizedEntityService.SaveLocalizedValueAsync(campaignCopy, x => x.Title, name, lang.Id);

                var shortDescription = await _localizationService.GetLocalizedAsync(campaign, x => x.Body, lang.Id, false, false);
                if (!string.IsNullOrEmpty(shortDescription))
                    await _localizedEntityService.SaveLocalizedValueAsync(campaignCopy, x => x.Body, shortDescription, lang.Id);
            }
        }

        protected int[] GetVibration(string vibration)
        {
            if (string.IsNullOrWhiteSpace(vibration))
                return new int[] { };

            var lst = new List<int>();
            var tokens = vibration.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in tokens)
            {
                if (int.TryParse(item, out var x))
                    lst.Add(x);
            }

            return lst.ToArray();
        }

        protected virtual async Task UpdateLocalesAsync(WebApiNotificationCampaign category, WebApiNotificationCampaignModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(category,
                    x => x.Title,
                    localized.Title,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(category,
                    x => x.Body,
                    localized.Body,
                    localized.LanguageId);
            }
        }

        #endregion

        #region Method

        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var searchModel = _pushNotificationCampaignModelFactory.PreparePushNotificationCampaignSearchModel(new WebApiNotificationCampaignSearchModel());
            return View(searchModel);
        }

        public async Task<IActionResult> GetList(WebApiNotificationCampaignSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageCampaigns))
                return await AccessDeniedDataTablesJson();

            var model = await _pushNotificationCampaignModelFactory.PreparePushNotificationCampaignListModelAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var model = await _pushNotificationCampaignModelFactory.PreparePushNotificationCampaignModelAsync(new WebApiNotificationCampaignModel(), null);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(WebApiNotificationCampaignModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var campaign = model.ToEntity<WebApiNotificationCampaign>();
                if (model.CustomerRoles.Any())
                    campaign.CustomerRoles = string.Join(",", model.CustomerRoles);

                if (model.DeviceTypes.Any())
                    campaign.DeviceTypes = string.Join(",", model.DeviceTypes);

                campaign.SendingWillStartOnUtc = _dateTimeHelper.ConvertToUtcTime(model.SendingWillStartOn, await _dateTimeHelper.GetCurrentTimeZoneAsync());
                campaign.CreatedOnUtc = DateTime.UtcNow;

                await _campaignService.InsertPushNotificationCampaignAsync(campaign);

                await UpdateLocalesAsync(campaign, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = campaign.Id });
            }

            model = await _pushNotificationCampaignModelFactory.PreparePushNotificationCampaignModelAsync(model, null);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = await _campaignService.GetPushNotificationCampaignByIdAsync(id);

            if (campaign == null || campaign.Deleted)
                return RedirectToAction("List");

            var model = await _pushNotificationCampaignModelFactory.PreparePushNotificationCampaignModelAsync(null, campaign);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(WebApiNotificationCampaignModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = await _campaignService.GetPushNotificationCampaignByIdAsync(model.Id);

            if (campaign == null || campaign.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                campaign = model.ToEntity(campaign);
                if (model.CustomerRoles.Any())
                    campaign.CustomerRoles = string.Join(",", model.CustomerRoles);
                else
                    campaign.CustomerRoles = null;

                if (model.DeviceTypes.Any())
                    campaign.DeviceTypes = string.Join(",", model.DeviceTypes);
                else
                    campaign.DeviceTypes = null;

                campaign.SendingWillStartOnUtc = _dateTimeHelper.ConvertToUtcTime(model.SendingWillStartOn, await _dateTimeHelper.GetCurrentTimeZoneAsync());

                await _campaignService.UpdatePushNotificationCampaignAsync(campaign);

                await UpdateLocalesAsync(campaign, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = campaign.Id });
            }

            model = await _pushNotificationCampaignModelFactory.PreparePushNotificationCampaignModelAsync(model, campaign);
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = await _campaignService.GetPushNotificationCampaignByIdAsync(id);
            if (campaign == null || campaign.Deleted)
                return RedirectToAction("List");

            await _campaignService.DeletePushNotificationCampaignAsync(campaign);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Deleted"));

            return RedirectToAction("List");
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> CopyCampaign(WebApiNotificationCampaignModel model)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var copyModel = model.CopyPushNotificationCampaignModel;
            try
            {
                var originalCampaign = await _campaignService.GetPushNotificationCampaignByIdAsync(copyModel.Id);

                var newCampaign = originalCampaign.Clone();
                newCampaign.Name = copyModel.Name;
                newCampaign.CreatedOnUtc = DateTime.UtcNow;
                newCampaign.SendingWillStartOnUtc = _dateTimeHelper.ConvertToUtcTime(copyModel.SendingWillStartOnUtc, await _dateTimeHelper.GetCurrentTimeZoneAsync());

                await _campaignService.InsertPushNotificationCampaignAsync(newCampaign);

                await CopyLocalizationDataAsync(originalCampaign, newCampaign);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Copied"));

                return RedirectToAction("Edit", new { id = newCampaign.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = copyModel.Id });
            }
        }

        #endregion
    }
}
