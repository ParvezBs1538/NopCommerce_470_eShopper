using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
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
    public class WebApiNotificationTemplateController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPushNotificationTemplateModelFactory _pushNotificationTemplateModelFactory;
        private readonly IPushNotificationTemplateService _pushNotificationTemplateService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public WebApiNotificationTemplateController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPushNotificationTemplateModelFactory pushNotificationTemplateModelFactory,
            IPushNotificationTemplateService pushNotificationTemplateService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            ILocalizedEntityService localizedEntityService,
            IPermissionService permissionService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _pushNotificationTemplateModelFactory = pushNotificationTemplateModelFactory;
            _pushNotificationTemplateService = pushNotificationTemplateService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _localizedEntityService = localizedEntityService;
        }

        #endregion

        #region Utilities

        protected virtual async Task UpdateLocalesAsync(WebApiNotificationTemplate category, WebApiNotificationTemplateModel model)
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

        protected virtual async Task SaveStoreMappingsAsync(WebApiNotificationTemplate pushNotificationTemplate, WebApiNotificationTemplateModel model)
        {
            pushNotificationTemplate.LimitedToStores = model.SelectedStoreIds.Any();

            //manage store mappings
            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(pushNotificationTemplate);
            foreach (var store in await _storeService.GetAllStoresAsync())
            {
                var existingStoreMapping = existingStoreMappings.FirstOrDefault(storeMapping => storeMapping.StoreId == store.Id);

                //new store mapping
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    if (existingStoreMapping == null)
                        await _storeMappingService.InsertStoreMappingAsync(pushNotificationTemplate, store.Id);
                }
                //or remove existing one
                else if (existingStoreMapping != null)
                    await _storeMappingService.DeleteStoreMappingAsync(existingStoreMapping);
            }
        }

        #endregion

        #region Methods        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageTemplates))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageTemplates))
                return AccessDeniedView();

            var searchModel = await _pushNotificationTemplateModelFactory.PreparePushNotificationTemplateSearchModelAsync(new WebApiNotificationTemplateSearchModel());
            return View(searchModel);
        }

        public virtual async Task<IActionResult> GetList(WebApiNotificationTemplateSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageTemplates))
                return await AccessDeniedDataTablesJson();

            var model = await _pushNotificationTemplateModelFactory.PreparePushNotificationTemplateListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageTemplates))
                return AccessDeniedView();

            var pushNotificationTemplate = await _pushNotificationTemplateService.GetPushNotificationTemplateByIdAsync(id);
            if (pushNotificationTemplate == null)
                return RedirectToAction("List");

            var model = await _pushNotificationTemplateModelFactory.PreparePushNotificationTemplateModelAsync(null, pushNotificationTemplate);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(WebApiNotificationTemplateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageTemplates))
                return AccessDeniedView();

            var pushNotificationTemplate = await _pushNotificationTemplateService.GetPushNotificationTemplateByIdAsync(model.Id);
            if (pushNotificationTemplate == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                pushNotificationTemplate = model.ToEntity(pushNotificationTemplate);

                await _pushNotificationTemplateService.UpdatePushNotificationTemplateAsync(pushNotificationTemplate);

                await SaveStoreMappingsAsync(pushNotificationTemplate, model);

                await UpdateLocalesAsync(pushNotificationTemplate, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = pushNotificationTemplate.Id });
            }

            model = await _pushNotificationTemplateModelFactory.PreparePushNotificationTemplateModelAsync(model, pushNotificationTemplate);

            return View(model);
        }

        #endregion
    }
}
