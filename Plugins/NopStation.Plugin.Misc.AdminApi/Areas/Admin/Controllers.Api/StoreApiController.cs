using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Stores;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Stores;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/store/[action]")]
public partial class StoreApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IStoreModelFactory _storeModelFactory;
    private readonly IStoreService _storeService;

    #endregion

    #region Ctor

    public StoreApiController(ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreModelFactory storeModelFactory,
        IStoreService storeService)
    {
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeModelFactory = storeModelFactory;
        _storeService = storeService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(Store store, StoreModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(store,
                x => x.Name,
                localized.Name,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(store,
                x => x.DefaultTitle,
                localized.DefaultTitle,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(store,
                x => x.DefaultMetaDescription,
                localized.DefaultMetaDescription,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(store,
                x => x.DefaultMetaKeywords,
                localized.DefaultMetaKeywords,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(store,
                x => x.HomepageDescription,
                localized.HomepageDescription,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(store,
                x => x.HomepageTitle,
                localized.HomepageTitle,
                localized.LanguageId);
        }
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _storeModelFactory.PrepareStoreSearchModelAsync(new StoreSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> StoreList([FromBody] BaseQueryModel<StoreSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _storeModelFactory.PrepareStoreListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _storeModelFactory.PrepareStoreModelAsync(new StoreModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<StoreModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var store = model.ToEntity<Store>();

            //ensure we have "/" at the end
            if (!store.Url.EndsWith('/'))
                store.Url += "/";

            await _storeService.InsertStoreAsync(store);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewStore",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewStore"), store.Id), store);

            //locales
            await UpdateLocalesAsync(store, model);

            return Created(store.Id, await _localizationService.GetResourceAsync("Admin.Configuration.Stores.Added"));
        }

        //prepare model
        model = await _storeModelFactory.PrepareStoreModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
            return AdminApiAccessDenied();

        //try to get a store with the specified id
        var store = await _storeService.GetStoreByIdAsync(id);
        if (store == null)
            return NotFound("No store found with the specified id");

        //prepare model
        var model = await _storeModelFactory.PrepareStoreModelAsync(null, store);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<StoreModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a store with the specified id
        var store = await _storeService.GetStoreByIdAsync(model.Id);
        if (store == null)
            return NotFound("No store found with the specified id");

        if (ModelState.IsValid)
        {
            store = model.ToEntity(store);

            //ensure we have "/" at the end
            if (!store.Url.EndsWith('/'))
                store.Url += "/";

            await _storeService.UpdateStoreAsync(store);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditStore",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditStore"), store.Id), store);

            //locales
            await UpdateLocalesAsync(store, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Stores.Updated"));
        }

        //prepare model
        model = await _storeModelFactory.PrepareStoreModelAsync(model, store, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
            return AdminApiAccessDenied();

        //try to get a store with the specified id
        var store = await _storeService.GetStoreByIdAsync(id);
        if (store == null)
            return NotFound("No store found with the specified id");

        try
        {
            await _storeService.DeleteStoreAsync(store);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteStore",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteStore"), store.Id), store);

            //when we delete a store we should also ensure that all "per store" settings will also be deleted
            var settingsToDelete = (await _settingService
                .GetAllSettingsAsync())
                .Where(s => s.StoreId == id)
                .ToList();
            await _settingService.DeleteSettingsAsync(settingsToDelete);

            //when we had two stores and now have only one store, we also should delete all "per store" settings
            var allStores = await _storeService.GetAllStoresAsync();
            if (allStores.Count == 1)
            {
                settingsToDelete = (await _settingService
                    .GetAllSettingsAsync())
                    .Where(s => s.StoreId == allStores[0].Id)
                    .ToList();
                await _settingService.DeleteSettingsAsync(settingsToDelete);
            }

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Stores.Deleted"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    #endregion
}