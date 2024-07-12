using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.SMS.Vonage.Areas.Admin.Factories;
using NopStation.Plugin.SMS.Vonage.Areas.Admin.Models;
using NopStation.Plugin.SMS.Vonage.Domains;
using NopStation.Plugin.SMS.Vonage.Services;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.Vonage.Areas.Admin.Controllers
{
    public class VonageSmsTemplateController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISmsTemplateModelFactory _smsTemplateModelFactory;
        private readonly ISmsTemplateService _smsTemplateService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IAclService _aclService;
        private readonly ICustomerService _customerService;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public VonageSmsTemplateController(IPermissionService permissionService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ISmsTemplateModelFactory smsTemplateModelFactory,
            ISmsTemplateService smsTemplateService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            ILocalizedEntityService localizedEntityService,
            IAclService aclService,
            ICustomerService customerService,
            IStaticCacheManager staticCacheManager)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _smsTemplateModelFactory = smsTemplateModelFactory;
            _smsTemplateService = smsTemplateService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _localizedEntityService = localizedEntityService;
            _aclService = aclService;
            _customerService = customerService;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utilities

        protected virtual void UpdateLocales(SmsTemplate smsTemplate, SmsTemplateModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValueAsync(smsTemplate,
                    x => x.Body,
                    localized.Body,
                    localized.LanguageId);
            }
        }

        protected virtual async Task SaveSmsTemplateAclAsync(SmsTemplate smsTemplate, SmsTemplateModel model)
        {
            smsTemplate.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            await _smsTemplateService.UpdateSmsTemplateAsync(smsTemplate);

            var existingAclRecords = await _aclService.GetAclRecordsAsync(smsTemplate);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        await _aclService.InsertAclRecordAsync(smsTemplate, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
                }
            }
        }

        protected virtual async Task SaveStoreMappingsAsync(SmsTemplate smsTemplate, SmsTemplateModel model)
        {
            smsTemplate.LimitedToStores = model.SelectedStoreIds.Any();

            //manage store mappings
            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(smsTemplate);
            foreach (var store in await _storeService.GetAllStoresAsync())
            {
                var existingStoreMapping = existingStoreMappings.FirstOrDefault(storeMapping => storeMapping.StoreId == store.Id);

                //new store mapping
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    if (existingStoreMapping == null)
                        await _storeMappingService.InsertStoreMappingAsync(smsTemplate, store.Id);
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
            if (!await _permissionService.AuthorizeAsync(VonagePermissionProvider.ManageTemplates))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(VonagePermissionProvider.ManageTemplates))
                return AccessDeniedView();

            var searchModel = _smsTemplateModelFactory.PrepareSmsTemplateSearchModel(new SmsTemplateSearchModel());
            return View(searchModel);
        }

        public virtual async Task<IActionResult> GetList(SmsTemplateSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(VonagePermissionProvider.ManageTemplates))
                return await AccessDeniedDataTablesJson();

            var model = await _smsTemplateModelFactory.PrepareSmsTemplateListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(VonagePermissionProvider.ManageTemplates))
                return AccessDeniedView();

            var smsTemplate = await _smsTemplateService.GetSmsTemplateByIdAsync(id);
            if (smsTemplate == null)
                return RedirectToAction("List");

            var model = await _smsTemplateModelFactory.PrepareSmsTemplateModelAsync(null, smsTemplate);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(SmsTemplateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(VonagePermissionProvider.ManageTemplates))
                return AccessDeniedView();

            var smsTemplate = await _smsTemplateService.GetSmsTemplateByIdAsync(model.Id);
            if (smsTemplate == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                smsTemplate = model.ToEntity(smsTemplate);
                await _smsTemplateService.UpdateSmsTemplateAsync(smsTemplate);

                await SaveSmsTemplateAclAsync(smsTemplate, model);

                await SaveStoreMappingsAsync(smsTemplate, model);

                UpdateLocales(smsTemplate, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.VonageSms.SmsTemplates.Updated"));

                await _staticCacheManager.ClearAsync();

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = smsTemplate.Id });
            }

            model = await _smsTemplateModelFactory.PrepareSmsTemplateModelAsync(model, smsTemplate);
            return View(model);
        }

        #endregion
    }
}
