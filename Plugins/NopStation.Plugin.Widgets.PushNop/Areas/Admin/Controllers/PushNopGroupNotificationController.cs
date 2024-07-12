using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.ProgressiveWebApp;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.PushNop.Domains;
using NopStation.Plugin.Widgets.PushNop.Services;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Controllers
{
    public class PushNopGroupNotificationController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISmartGroupNotificationModelFactory _smartGroupNotificationModelFactory;
        private readonly ISmartGroupNotificationService _smartGroupNotificationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPushNotificationTokenProvider _pushNotificationTokenProvider;
        private readonly ITokenizer _tokenizer;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;

        #endregion

        #region Ctor

        public PushNopGroupNotificationController(ILocalizationService localizationService,
            INotificationService notificationService,
            ISmartGroupNotificationModelFactory smartGroupNotificationModelFactory,
            ISmartGroupNotificationService smartGroupNotificationService,
            IDateTimeHelper dateTimeHelper,
            IPermissionService permissionService,
            ILocalizedEntityService localizedEntityService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IPushNotificationTokenProvider pushNotificationTokenProvider,
            ITokenizer tokenizer,
            IPictureService pictureService,
            ILanguageService languageService,
            ProgressiveWebAppSettings progressiveWebAppSettings)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _smartGroupNotificationModelFactory = smartGroupNotificationModelFactory;
            _smartGroupNotificationService = smartGroupNotificationService;
            _dateTimeHelper = dateTimeHelper;
            _localizedEntityService = localizedEntityService;
            _workContext = workContext;
            _storeContext = storeContext;
            _pushNotificationTokenProvider = pushNotificationTokenProvider;
            _tokenizer = tokenizer;
            _pictureService = pictureService;
            _languageService = languageService;
            _progressiveWebAppSettings = progressiveWebAppSettings;
        }

        #endregion

        #region Utilities

        protected virtual async Task CopyLocalizationData(SmartGroupNotification smartGroupNotification, SmartGroupNotification smartGroupNotificationCopy)
        {
            var languages = await _languageService.GetAllLanguagesAsync(true);

            //localization
            foreach (var lang in languages)
            {
                var name = await _localizationService.GetLocalizedAsync(smartGroupNotification, x => x.Title, lang.Id, false, false);
                if (!string.IsNullOrEmpty(name))
                    await _localizedEntityService.SaveLocalizedValueAsync(smartGroupNotificationCopy, x => x.Title, name, lang.Id);

                var shortDescription = await _localizationService.GetLocalizedAsync(smartGroupNotification, x => x.Body, lang.Id, false, false);
                if (!string.IsNullOrEmpty(shortDescription))
                    await _localizedEntityService.SaveLocalizedValueAsync(smartGroupNotificationCopy, x => x.Body, shortDescription, lang.Id);
            }
        }

        protected virtual async Task UpdateLocales(SmartGroupNotification smartGroupNotification, GroupNotificationModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(smartGroupNotification,
                    x => x.Title,
                    localized.Title,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(smartGroupNotification,
                    x => x.Body,
                    localized.Body,
                    localized.LanguageId);
            }
        }

        #endregion

        #region Method

        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroupNotifications))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroupNotifications))
                return AccessDeniedView();

            var searchModel = await _smartGroupNotificationModelFactory.PrepareSmartGroupNotificationSearchModelAsync(new GroupNotificationSearchModel());
            return View(searchModel);
        }

        public async Task<IActionResult> GetList(GroupNotificationSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroupNotifications))
                return await AccessDeniedDataTablesJson();

            var model = await _smartGroupNotificationModelFactory.PrepareSmartGroupNotificationListModelAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroupNotifications))
                return AccessDeniedView();

            var model = await _smartGroupNotificationModelFactory.PrepareSmartGroupNotificationModelAsync(new GroupNotificationModel(), null);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(GroupNotificationModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroupNotifications))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var smartGroupNotification = model.ToEntity<SmartGroupNotification>();

                smartGroupNotification.SendingWillStartOnUtc = _dateTimeHelper.ConvertToUtcTime(model.SendingWillStartOn, DateTimeKind.Local);
                smartGroupNotification.CreatedOnUtc = DateTime.UtcNow;

                await _smartGroupNotificationService.InsertSmartGroupNotificationAsync(smartGroupNotification);

                await UpdateLocales(smartGroupNotification, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.GroupNotifications.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = smartGroupNotification.Id });
            }

            model = await _smartGroupNotificationModelFactory.PrepareSmartGroupNotificationModelAsync(model, null);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroupNotifications))
                return AccessDeniedView();

            var campaign = await _smartGroupNotificationService.GetSmartGroupNotificationByIdAsync(id);

            if (campaign == null || campaign.Deleted)
                return RedirectToAction("List");

            var model = await _smartGroupNotificationModelFactory.PrepareSmartGroupNotificationModelAsync(null, campaign);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(GroupNotificationModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroupNotifications))
                return AccessDeniedView();

            var campaign = await _smartGroupNotificationService.GetSmartGroupNotificationByIdAsync(model.Id);

            if (campaign == null || campaign.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                campaign = model.ToEntity(campaign);
                campaign.SendingWillStartOnUtc = _dateTimeHelper.ConvertToUtcTime(model.SendingWillStartOn, DateTimeKind.Local);

                await _smartGroupNotificationService.UpdateSmartGroupNotificationAsync(campaign);

                await UpdateLocales(campaign, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.GroupNotifications.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = campaign.Id });
            }

            model = await _smartGroupNotificationModelFactory.PrepareSmartGroupNotificationModelAsync(model, campaign);
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroupNotifications))
                return AccessDeniedView();

            var campaign = await _smartGroupNotificationService.GetSmartGroupNotificationByIdAsync(id);
            if (campaign == null || campaign.Deleted)
                return RedirectToAction("List");

            await _smartGroupNotificationService.DeleteSmartGroupNotificationAsync(campaign);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.GroupNotifications.Deleted"));

            return RedirectToAction("List");
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> CopyCampaign(GroupNotificationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroupNotifications))
                return AccessDeniedView();

            var copyModel = model.CopySmartGroupNotificationModel;
            try
            {
                var originalCampaign = await _smartGroupNotificationService.GetSmartGroupNotificationByIdAsync(copyModel.Id);

                var newCampaign = originalCampaign.Clone();
                newCampaign.Name = copyModel.Name;
                newCampaign.CreatedOnUtc = DateTime.UtcNow;
                newCampaign.SendingWillStartOnUtc = _dateTimeHelper.ConvertToUtcTime(copyModel.SendingWillStartOnUtc, DateTimeKind.Local);

                await _smartGroupNotificationService.InsertSmartGroupNotificationAsync(newCampaign);

                await CopyLocalizationData(originalCampaign, newCampaign);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.GroupNotifications.Copied"));

                return RedirectToAction("Edit", new { id = newCampaign.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = copyModel.Id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetNotificationDetails(int id)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroupNotifications))
                return AccessDeniedView();

            var smartGroupNotification = await _smartGroupNotificationService.GetSmartGroupNotificationByIdAsync(id);
            if (smartGroupNotification == null || smartGroupNotification.Deleted)
                return Json(new { Result = false });

            var customer = await _workContext.GetCurrentCustomerAsync();
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            //tokens
            var commonTokens = new List<Token>();
            if (customer != null)
                await _pushNotificationTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            var tokens = new List<Token>(commonTokens);
            await _pushNotificationTokenProvider.AddStoreTokensAsync(tokens, await _storeContext.GetCurrentStoreAsync());

            var title = await _localizationService.GetLocalizedAsync(smartGroupNotification, mt => mt.Title, languageId);
            var body = await _localizationService.GetLocalizedAsync(smartGroupNotification, mt => mt.Body, languageId);

            var imageUrl = await _pictureService.GetPictureUrlAsync(smartGroupNotification.ImageId, showDefaultPicture: false);
            if (string.IsNullOrWhiteSpace(imageUrl))
                imageUrl = null;

            return Json(new
            {
                Result = true,
                Title = _tokenizer.Replace(title, tokens, true),
                Body = _tokenizer.Replace(body, tokens, true),
                Url = !string.IsNullOrWhiteSpace(smartGroupNotification.Url) ? _tokenizer.Replace(smartGroupNotification.Url, tokens, true) : null,
                Icon = !smartGroupNotification.UseDefaultIcon ? await _pictureService.GetPictureUrlAsync(smartGroupNotification.IconId, 80) :
                    await _pictureService.GetPictureUrlAsync(_progressiveWebAppSettings.DefaultIconId, 80),
                Image = imageUrl,
                Dir = (await _workContext.GetWorkingLanguageAsync()).Rtl ? "rtl" : "ltr"
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetImageUrlAsync(int imageId, int imageSize)
        {
            var url = await _pictureService.GetPictureUrlAsync(imageId, imageSize);
            return Json(new { url });
        }

        #endregion
    }
}
