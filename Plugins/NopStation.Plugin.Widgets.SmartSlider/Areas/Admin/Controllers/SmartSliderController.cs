using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Controllers;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartSliders.Domains;
using NopStation.Plugin.Widgets.SmartSliders.Services;

namespace NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Controllers;

public class SmartSliderController : BaseWidgetAdminController
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly IPictureService _pictureService;
    private readonly ISettingService _settingService;
    private readonly SmartSliderSettings _sliderSettings;
    private readonly ISmartSliderService _sliderService;
    private readonly INotificationService _notificationService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly ISmartSliderModelFactory _sliderModelFactory;
    private readonly IConditionModelFactory _conditionModelFactory;
    private readonly IStoreService _storeService;
    private readonly ICustomerService _customerService;
    private readonly IAclService _aclService;

    #endregion

    #region Ctor

    public SmartSliderController(IStoreContext storeContext,
        ILocalizedEntityService localizedEntityService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IPictureService pictureService,
        ISettingService settingService,
        SmartSliderSettings sliderSettings,
        ISmartSliderService sliderService,
        INotificationService notificationService,
        IStoreMappingService storeMappingService,
        ISmartSliderModelFactory sliderModelFactory,
        IConditionModelFactory conditionModelFactory,
        IStoreService storeService,
        ICustomerService customerService,
        IAclService aclService)
    {
        _storeContext = storeContext;
        _localizedEntityService = localizedEntityService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _pictureService = pictureService;
        _sliderSettings = sliderSettings;
        _settingService = settingService;
        _sliderService = sliderService;
        _notificationService = notificationService;
        _storeMappingService = storeMappingService;
        _sliderModelFactory = sliderModelFactory;
        _conditionModelFactory = conditionModelFactory;
        _storeService = storeService;
        _customerService = customerService;
        _aclService = aclService;
    }

    #endregion

    #region Utilities

    protected virtual async Task SaveStoreMappingsAsync(SmartSlider slider, SmartSliderModel model)
    {
        slider.LimitedToStores = model.SelectedStoreIds.Any();

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(slider);
        var allStores = await _storeService.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                    await _storeMappingService.InsertStoreMappingAsync(slider, store.Id);
            }
            else
            {
                //remove store
                var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                if (storeMappingToDelete != null)
                    await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
            }
        }
    }

    protected virtual async Task SaveSliderAclAsync(SmartSlider slider, SmartSliderModel model)
    {
        slider.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
        await _sliderService.UpdateSliderAsync(slider);

        var existingAclRecords = await _aclService.GetAclRecordsAsync(slider);
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        foreach (var customerRole in allCustomerRoles)
        {
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
            {
                //new role
                if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                    await _aclService.InsertAclRecordAsync(slider, customerRole.Id);
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

    protected virtual async Task UpdateLocalesAsync(SmartSliderItem sliderItem, SmartSliderItemModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(sliderItem,
                x => x.Title,
                localized.Title,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(sliderItem,
                x => x.Description,
                localized.Description,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(sliderItem,
                x => x.RedirectUrl,
                localized.RedirectUrl,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(sliderItem,
                x => x.ButtonText,
                localized.ButtonText,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(sliderItem,
                x => x.Text,
                localized.Text,
                localized.LanguageId);
        }
    }

    #endregion

    #region Methods

    #region Configuration

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        var model = await _sliderModelFactory.PrepareConfigurationModelAsync();

        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

        var sliderSettings = await _settingService.LoadSettingAsync<SmartSliderSettings>(storeScope);
        sliderSettings = model.ToSettings(sliderSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(sliderSettings, x => x.EnableSlider, model.EnableSlider_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(sliderSettings, x => x.EnableAjaxLoad, model.EnableAjaxLoad_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingAsync(sliderSettings, x => x.SupportedVideoExtensions);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion

    #region List

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        var model = await _sliderModelFactory.PrepareSliderSearchModelAsync(new SmartSliderSearchModel());

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> List(SmartSliderSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return await AccessDeniedDataTablesJson();

        var model = await _sliderModelFactory.PrepareSliderListModelAsync(searchModel);

        return Json(model);
    }

    #endregion

    #region Create / update / delete

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        var model = await _sliderModelFactory.PrepareSliderModelAsync(new SmartSliderModel(), null);

        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> Create(SmartSliderModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var slider = model.ToEntity<SmartSlider>();
            slider.CreatedOnUtc = DateTime.UtcNow;
            slider.UpdatedOnUtc = DateTime.UtcNow;

            await _sliderService.InsertSliderAsync(slider);

            await SaveStoreMappingsAsync(slider, model);

            //ACL (customer roles)
            await SaveSliderAclAsync(slider, model);

            await _sliderService.UpdateSliderAsync(slider);

            //update schedule mappings
            await UpdateScheduleMappingsAsync(model.Schedule, slider, async (slider) => await _sliderService.UpdateSliderAsync(slider));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Sliders.Created"));

            return continueEditing ?
                RedirectToAction("Edit", new { id = slider.Id }) :
                RedirectToAction("List");
        }

        model = await _sliderModelFactory.PrepareSliderModelAsync(model, null);

        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        var slider = await _sliderService.GetSliderByIdAsync(id);
        if (slider == null || slider.Deleted)
            return RedirectToAction("List");

        var model = await _sliderModelFactory.PrepareSliderModelAsync(null, slider);

        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Edit(SmartSliderModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        var slider = await _sliderService.GetSliderByIdAsync(model.Id);
        if (slider == null || slider.Deleted)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            slider = model.ToEntity(slider);
            slider.UpdatedOnUtc = DateTime.UtcNow;

            await _sliderService.UpdateSliderAsync(slider);

            await SaveStoreMappingsAsync(slider, model);

            //ACL (customer roles)
            await SaveSliderAclAsync(slider, model);

            await _sliderService.UpdateSliderAsync(slider);

            //update schedule mappings
            await UpdateScheduleMappingsAsync(model.Schedule, slider, async (slider) => await _sliderService.UpdateSliderAsync(slider));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Sliders.Updated"));

            return continueEditing ?
                RedirectToAction("Edit", new { id = model.Id }) :
                RedirectToAction("List");
        }

        model = await _sliderModelFactory.PrepareSliderModelAsync(model, slider);

        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        var slider = await _sliderService.GetSliderByIdAsync(id);
        if (slider == null || slider.Deleted)
            return RedirectToAction("List");

        await _sliderService.DeleteSliderAsync(slider);
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Sliders.Deleted"));

        return RedirectToAction("List");
    }

    #endregion

    #region Slider items

    public virtual async Task<IActionResult> SliderItemCreatePopup(int sliderId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        var slider = await _sliderService.GetSliderByIdAsync(sliderId)
            ?? throw new ArgumentException("No slider found with the specified id", nameof(sliderId));

        //prepare model
        var model = await _sliderModelFactory.PrepareSliderItemModelAsync(new SmartSliderItemModel(), null, slider);

        return View(model);
    }

    [EditAccess, HttpPost]
    public virtual async Task<IActionResult> SliderItemCreatePopup(SmartSliderItemModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        //try to get a slider with the specified id
        var slider = await _sliderService.GetSliderByIdAsync(model.SliderId)
            ?? throw new ArgumentException("No slider found with the specified id");

        if (ModelState.IsValid)
        {
            //fill entity from model
            var sliderItem = model.ToEntity<SmartSliderItem>();

            await _sliderService.InsertSliderItemAsync(sliderItem);
            await UpdateLocalesAsync(sliderItem, model);

            ViewBag.RefreshPage = true;

            return View(model);
        }

        //prepare model
        model = await _sliderModelFactory.PrepareSliderItemModelAsync(model, null, slider);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public virtual async Task<IActionResult> SliderItemEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        //try to get a predefined slider value with the specified id
        var sliderItem = await _sliderService.GetSliderItemByIdAsync(id)
            ?? throw new ArgumentException("No slider item found with the specified id");

        //try to get a slider with the specified id
        var slider = await _sliderService.GetSliderByIdAsync(sliderItem.SliderId)
            ?? throw new ArgumentException("No slider found with the specified id");

        //prepare model
        var model = await _sliderModelFactory.PrepareSliderItemModelAsync(null, sliderItem, slider);

        return View(model);
    }

    [EditAccess, HttpPost]
    public virtual async Task<IActionResult> SliderItemEditPopup(SmartSliderItemModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        //try to get a predefined slider value with the specified id
        var sliderItem = await _sliderService.GetSliderItemByIdAsync(model.Id)
            ?? throw new ArgumentException("No slider item found with the specified id");

        //try to get a slider with the specified id
        var slider = await _sliderService.GetSliderByIdAsync(sliderItem.SliderId)
            ?? throw new ArgumentException("No slider found with the specified id");

        if (ModelState.IsValid)
        {
            sliderItem = model.ToEntity(sliderItem);
            await _sliderService.UpdateSliderItemAsync(sliderItem);

            await UpdateLocalesAsync(sliderItem, model);
            ViewBag.RefreshPage = true;

            return View(model);
        }

        //prepare model
        model = await _sliderModelFactory.PrepareSliderItemModelAsync(model, sliderItem, slider, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [EditAccessAjax, HttpPost]
    public async Task<IActionResult> SliderItemDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return await AccessDeniedDataTablesJson();

        var sliderItem = await _sliderService.GetSliderItemByIdAsync(id)
            ?? throw new ArgumentException("No slider item found with the specified id");

        await _sliderService.DeleteSliderItemAsync(sliderItem);

        return new NullJsonResult();
    }

    [HttpPost]
    public async Task<IActionResult> SliderItemList(SmartSliderItemSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return await AccessDeniedDataTablesJson();

        var model = await _sliderModelFactory.PrepareSliderItemListModelAsync(searchModel);
        return Json(model);
    }

    #endregion

    #region Widget zone mappings

    [HttpPost]
    public virtual async Task<IActionResult> WidgetZoneList(WidgetZoneSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return await AccessDeniedDataTablesJson();

        var slider = await _sliderService.GetSliderByIdAsync(searchModel.EntityId);
        if (slider == null || slider.Deleted)
            throw new ArgumentException("No slider found with the specified id");

        var model = await base.WidgetZoneListAsync(searchModel, slider);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> WidgetZoneCreate(WidgetZoneModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return await AccessDeniedDataTablesJson();

        var slider = await _sliderService.GetSliderByIdAsync(model.EntityId);
        if (slider == null || slider.Deleted)
            throw new ArgumentException("No slider found with the specified id");

        await base.WidgetZoneCreateAsync(model, slider);

        return Json(new { Result = true });
    }

    [HttpPost]
    public virtual async Task<IActionResult> WidgetZoneEdit(WidgetZoneModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return await AccessDeniedDataTablesJson();

        var slider = await _sliderService.GetSliderByIdAsync(model.EntityId);
        if (slider == null || slider.Deleted)
            throw new ArgumentException("No slider found with the specified id");

        await base.WidgetZoneEditAsync(model, slider);

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> WidgetZoneDelete(int id, int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return await AccessDeniedDataTablesJson();

        var slider = await _sliderService.GetSliderByIdAsync(entityId);
        if (slider == null || slider.Deleted)
            throw new ArgumentException("No slider found with the specified id");

        await base.WidgetZoneDeleteAsync(id, slider);

        return new NullJsonResult();
    }

    #endregion

    #region Customer condition mappings

    [HttpPost]
    public virtual async Task<IActionResult> CustomerConditionList(CustomerConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return await AccessDeniedDataTablesJson();

        //try to get a slider with the specified id
        var slider = await _sliderService.GetSliderByIdAsync(searchModel.EntityId);
        if (slider == null || slider.Deleted)
            throw new ArgumentException("No slider found with the specified id");

        //prepare model
        var model = await base.CustomerConditionListAsync(searchModel, slider);

        return Json(model);
    }

    public virtual async Task<IActionResult> CustomerConditionDelete(int id, int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        //try to get a slider with the specified id
        var slider = await _sliderService.GetSliderByIdAsync(entityId);
        if (slider == null || slider.Deleted)
            throw new ArgumentException("No slider found with the specified id");

        await base.CustomerConditionDeleteAsync(id, slider);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> CustomerConditionAddPopup(int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        //try to get a slider with the specified id
        var slider = await _sliderService.GetSliderByIdAsync(entityId);
        if (slider == null || slider.Deleted)
            throw new ArgumentException("No slider found with the specified id");

        //prepare model
        var model = await _conditionModelFactory.PrepareAddCustomerToConditionSearchModelAsync(new AddCustomerToConditionSearchModel(), slider);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CustomerConditionAddList(AddCustomerToConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _conditionModelFactory.PrepareAddCustomerToConditionListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> CustomerConditionAddPopup(AddCustomerToConditionModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        //try to get a slider with the specified id
        var slider = await _sliderService.GetSliderByIdAsync(model.EntityId);
        if (slider == null || slider.Deleted)
            throw new ArgumentException("No slider found with the specified id");

        await base.CustomerConditionAddAsync(model, slider);

        ViewBag.RefreshPage = true;

        return View(new AddCustomerToConditionSearchModel());
    }

    #endregion

    #region Product condition mappings

    [HttpPost]
    public virtual async Task<IActionResult> ProductConditionList(ProductConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return await AccessDeniedDataTablesJson();

        //try to get a slider with the specified id
        var slider = await _sliderService.GetSliderByIdAsync(searchModel.EntityId);
        if (slider == null || slider.Deleted)
            throw new ArgumentException("No slider found with the specified id");

        //prepare model
        var model = await base.ProductConditionListAsync(searchModel, slider);

        return Json(model);
    }

    public virtual async Task<IActionResult> ProductConditionDelete(int id, int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        //try to get a slider with the specified id
        var slider = await _sliderService.GetSliderByIdAsync(entityId);
        if (slider == null || slider.Deleted)
            throw new ArgumentException("No slider found with the specified id");

        await base.ProductConditionDeleteAsync(id, slider);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> ProductConditionAddPopup(int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        //try to get a slider with the specified id
        var slider = await _sliderService.GetSliderByIdAsync(entityId);
        if (slider == null || slider.Deleted)
            throw new ArgumentException("No slider found with the specified id");

        //prepare model
        var model = await _conditionModelFactory.PrepareAddProductToConditionSearchModelAsync(new AddProductToConditionSearchModel(), slider);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductConditionAddList(AddProductToConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _conditionModelFactory.PrepareAddProductToConditionListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> ProductConditionAddPopup(AddProductToConditionModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
            return AccessDeniedView();

        //try to get a slider with the specified id
        var slider = await _sliderService.GetSliderByIdAsync(model.EntityId);
        if (slider == null || slider.Deleted)
            throw new ArgumentException("No slider found with the specified id");

        await base.ProductConditionAddAsync(model, slider);

        ViewBag.RefreshPage = true;

        return View(new AddProductToConditionSearchModel());
    }

    #endregion

    #endregion
}