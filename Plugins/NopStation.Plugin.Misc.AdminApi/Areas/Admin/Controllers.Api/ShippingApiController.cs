using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Shipping.Pickup;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Shipping;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/shipping/[action]")]
public partial class ShippingApiController : BaseAdminApiController
{
    #region Fields

    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IDateRangeService _dateRangeService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IPermissionService _permissionService;
    private readonly IPickupPluginManager _pickupPluginManager;
    private readonly ISettingService _settingService;
    private readonly IShippingModelFactory _shippingModelFactory;
    private readonly IShippingPluginManager _shippingPluginManager;
    private readonly IShippingService _shippingService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkContext _workContext;
    private readonly ShippingSettings _shippingSettings;

    #endregion

    #region Ctor

    public ShippingApiController(IAddressService addressService,
        ICountryService countryService,
        ICustomerActivityService customerActivityService,
        IDateRangeService dateRangeService,
        IEventPublisher eventPublisher,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IPermissionService permissionService,
        IPickupPluginManager pickupPluginManager,
        ISettingService settingService,
        IShippingModelFactory shippingModelFactory,
        IShippingPluginManager shippingPluginManager,
        IShippingService shippingService,
        IGenericAttributeService genericAttributeService,
        IWorkContext workContext,
        ShippingSettings shippingSettings)
    {
        _addressService = addressService;
        _countryService = countryService;
        _customerActivityService = customerActivityService;
        _dateRangeService = dateRangeService;
        _eventPublisher = eventPublisher;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _permissionService = permissionService;
        _pickupPluginManager = pickupPluginManager;
        _settingService = settingService;
        _shippingModelFactory = shippingModelFactory;
        _shippingPluginManager = shippingPluginManager;
        _shippingService = shippingService;
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
        _shippingSettings = shippingSettings;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(ShippingMethod shippingMethod, ShippingMethodModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(shippingMethod, x => x.Name, localized.Name, localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(shippingMethod, x => x.Description, localized.Description, localized.LanguageId);
        }
    }

    protected virtual async Task UpdateLocalesAsync(DeliveryDate deliveryDate, DeliveryDateModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(deliveryDate, x => x.Name, localized.Name, localized.LanguageId);
        }
    }

    protected virtual async Task UpdateLocalesAsync(ProductAvailabilityRange productAvailabilityRange, ProductAvailabilityRangeModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(productAvailabilityRange, x => x.Name, localized.Name, localized.LanguageId);
        }
    }

    #endregion

    #region Shipping rate computation methods

    public virtual async Task<IActionResult> Providers()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _shippingModelFactory.PrepareShippingProviderSearchModelAsync(new ShippingProviderSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Providers([FromBody] BaseQueryModel<ShippingProviderSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _shippingModelFactory.PrepareShippingProviderListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProviderUpdate([FromBody] BaseQueryModel<ShippingProviderModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var srcm = await _shippingPluginManager.LoadPluginBySystemNameAsync(model.SystemName);
        if (_shippingPluginManager.IsPluginActive(srcm))
        {
            if (!model.IsActive)
            {
                //mark as disabled
                _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Remove(srcm.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_shippingSettings);
            }
        }
        else
        {
            if (model.IsActive)
            {
                //mark as active
                _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(srcm.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_shippingSettings);
            }
        }

        var pluginDescriptor = srcm.PluginDescriptor;

        //display order
        pluginDescriptor.DisplayOrder = model.DisplayOrder;

        //update the description file
        pluginDescriptor.Save();

        //raise event
        await _eventPublisher.PublishAsync(new PluginUpdatedEvent(pluginDescriptor));

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Pickup point providers

    public virtual async Task<IActionResult> PickupPointProviders()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _shippingModelFactory.PreparePickupPointProviderSearchModelAsync(new PickupPointProviderSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PickupPointProviders([FromBody] BaseQueryModel<PickupPointProviderSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _shippingModelFactory.PreparePickupPointProviderListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PickupPointProviderUpdate([FromBody] BaseQueryModel<PickupPointProviderModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var pickupPointProvider = await _pickupPluginManager.LoadPluginBySystemNameAsync(model.SystemName);
        if (_pickupPluginManager.IsPluginActive(pickupPointProvider))
        {
            if (!model.IsActive)
            {
                //mark as disabled
                _shippingSettings.ActivePickupPointProviderSystemNames.Remove(pickupPointProvider.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_shippingSettings);
            }
        }
        else
        {
            if (model.IsActive)
            {
                //mark as active
                _shippingSettings.ActivePickupPointProviderSystemNames.Add(pickupPointProvider.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_shippingSettings);
            }
        }

        var pluginDescriptor = pickupPointProvider.PluginDescriptor;
        pluginDescriptor.DisplayOrder = model.DisplayOrder;

        //update the description file
        pluginDescriptor.Save();

        //raise event
        await _eventPublisher.PublishAsync(new PluginUpdatedEvent(pluginDescriptor));

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Shipping methods

    public virtual async Task<IActionResult> Methods()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _shippingModelFactory.PrepareShippingMethodSearchModelAsync(new ShippingMethodSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Methods([FromBody] BaseQueryModel<ShippingMethodSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _shippingModelFactory.PrepareShippingMethodListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> CreateMethod()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _shippingModelFactory.PrepareShippingMethodModelAsync(new ShippingMethodModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CreateMethod([FromBody] BaseQueryModel<ShippingMethodModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var sm = model.ToEntity<ShippingMethod>();
            await _shippingService.InsertShippingMethodAsync(sm);

            //locales
            await UpdateLocalesAsync(sm, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Methods.Added"));
        }

        //prepare model
        model = await _shippingModelFactory.PrepareShippingMethodModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> EditMethod(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //try to get a shipping method with the specified id
        var shippingMethod = await _shippingService.GetShippingMethodByIdAsync(id);
        if (shippingMethod == null)
            return NotFound("No shipping method found with the specified id");

        //prepare model
        var model = await _shippingModelFactory.PrepareShippingMethodModelAsync(null, shippingMethod);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditMethod([FromBody] BaseQueryModel<ShippingMethodModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a shipping method with the specified id
        var shippingMethod = await _shippingService.GetShippingMethodByIdAsync(model.Id);
        if (shippingMethod == null)
            return NotFound("No shipping method found with the specified id");

        if (ModelState.IsValid)
        {
            shippingMethod = model.ToEntity(shippingMethod);
            await _shippingService.UpdateShippingMethodAsync(shippingMethod);

            //locales
            await UpdateLocalesAsync(shippingMethod, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Methods.Updated"));
        }

        //prepare model
        model = await _shippingModelFactory.PrepareShippingMethodModelAsync(model, shippingMethod, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> DeleteMethod(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //try to get a shipping method with the specified id
        var shippingMethod = await _shippingService.GetShippingMethodByIdAsync(id);
        if (shippingMethod == null)
            return NotFound("No shipping method found with the specified id");

        await _shippingService.DeleteShippingMethodAsync(shippingMethod);

        return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Methods.Deleted"));
    }

    #endregion

    #region Dates and ranges

    public virtual async Task<IActionResult> DatesAndRanges()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _shippingModelFactory.PrepareDatesRangesSearchModelAsync(new DatesRangesSearchModel());

        return OkWrap(model);
    }

    #endregion

    #region Delivery dates

    [HttpPost]
    public virtual async Task<IActionResult> DeliveryDates([FromBody] BaseQueryModel<DeliveryDateSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _shippingModelFactory.PrepareDeliveryDateListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> CreateDeliveryDate()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _shippingModelFactory.PrepareDeliveryDateModelAsync(new DeliveryDateModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CreateDeliveryDate([FromBody] BaseQueryModel<DeliveryDateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var deliveryDate = model.ToEntity<DeliveryDate>();
            await _dateRangeService.InsertDeliveryDateAsync(deliveryDate);

            //locales
            await UpdateLocalesAsync(deliveryDate, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.DeliveryDates.Added"));
        }

        //prepare model
        model = await _shippingModelFactory.PrepareDeliveryDateModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> EditDeliveryDate(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //try to get a delivery date with the specified id
        var deliveryDate = await _dateRangeService.GetDeliveryDateByIdAsync(id);
        if (deliveryDate == null)
            return NotFound("No delivery date found with the specified id");

        //prepare model
        var model = await _shippingModelFactory.PrepareDeliveryDateModelAsync(null, deliveryDate);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditDeliveryDate([FromBody] BaseQueryModel<DeliveryDateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a delivery date with the specified id
        var deliveryDate = await _dateRangeService.GetDeliveryDateByIdAsync(model.Id);
        if (deliveryDate == null)
            return NotFound("No delivery date found with the specified id");

        if (ModelState.IsValid)
        {
            deliveryDate = model.ToEntity(deliveryDate);
            await _dateRangeService.UpdateDeliveryDateAsync(deliveryDate);

            //locales
            await UpdateLocalesAsync(deliveryDate, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.DeliveryDates.Updated"));
        }

        //prepare model
        model = await _shippingModelFactory.PrepareDeliveryDateModelAsync(model, deliveryDate, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> DeleteDeliveryDate(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //try to get a delivery date with the specified id
        var deliveryDate = await _dateRangeService.GetDeliveryDateByIdAsync(id);
        if (deliveryDate == null)
            return NotFound("No delivery date found with the specified id");

        await _dateRangeService.DeleteDeliveryDateAsync(deliveryDate);

        return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.DeliveryDates.Deleted"));
    }

    #endregion

    #region Product availability ranges

    [HttpPost]
    public virtual async Task<IActionResult> ProductAvailabilityRanges([FromBody] BaseQueryModel<ProductAvailabilityRangeSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _shippingModelFactory.PrepareProductAvailabilityRangeListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> CreateProductAvailabilityRange()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _shippingModelFactory.PrepareProductAvailabilityRangeModelAsync(new ProductAvailabilityRangeModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CreateProductAvailabilityRange([FromBody] BaseQueryModel<ProductAvailabilityRangeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var productAvailabilityRange = model.ToEntity<ProductAvailabilityRange>();
            await _dateRangeService.InsertProductAvailabilityRangeAsync(productAvailabilityRange);

            //locales
            await UpdateLocalesAsync(productAvailabilityRange, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.ProductAvailabilityRanges.Added"));
        }

        //prepare model
        model = await _shippingModelFactory.PrepareProductAvailabilityRangeModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> EditProductAvailabilityRange(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //try to get a product availability range with the specified id
        var productAvailabilityRange = await _dateRangeService.GetProductAvailabilityRangeByIdAsync(id);
        if (productAvailabilityRange == null)
            return NotFound("No product availability range found with the specified id");

        //prepare model
        var model = await _shippingModelFactory.PrepareProductAvailabilityRangeModelAsync(null, productAvailabilityRange);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditProductAvailabilityRange([FromBody] BaseQueryModel<ProductAvailabilityRangeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product availability range with the specified id
        var productAvailabilityRange = await _dateRangeService.GetProductAvailabilityRangeByIdAsync(model.Id);
        if (productAvailabilityRange == null)
            return NotFound("No product availability range found with the specified id");

        if (ModelState.IsValid)
        {
            productAvailabilityRange = model.ToEntity(productAvailabilityRange);
            await _dateRangeService.UpdateProductAvailabilityRangeAsync(productAvailabilityRange);

            //locales
            await UpdateLocalesAsync(productAvailabilityRange, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.ProductAvailabilityRanges.Updated"));
        }

        //prepare model
        model = await _shippingModelFactory.PrepareProductAvailabilityRangeModelAsync(model, productAvailabilityRange, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> DeleteProductAvailabilityRange(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //try to get a product availability range with the specified id
        var productAvailabilityRange = await _dateRangeService.GetProductAvailabilityRangeByIdAsync(id);
        if (productAvailabilityRange == null)
            return RedirectToAction("DatesAndRanges");

        await _dateRangeService.DeleteProductAvailabilityRangeAsync(productAvailabilityRange);

        return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.ProductAvailabilityRanges.Deleted"));
    }

    #endregion

    #region Warehouses

    public virtual async Task<IActionResult> Warehouses()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _shippingModelFactory.PrepareWarehouseSearchModelAsync(new WarehouseSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Warehouses([FromBody] BaseQueryModel<WarehouseSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _shippingModelFactory.PrepareWarehouseListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> CreateWarehouse()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _shippingModelFactory.PrepareWarehouseModelAsync(new WarehouseModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CreateWarehouse([FromBody] BaseQueryModel<WarehouseModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var address = model.Address.ToEntity<Address>();
            address.CreatedOnUtc = DateTime.UtcNow;
            await _addressService.InsertAddressAsync(address);

            //fill entity from model
            var warehouse = model.ToEntity<Warehouse>();
            warehouse.AddressId = address.Id;

            await _shippingService.InsertWarehouseAsync(warehouse);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewWarehouse",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewWarehouse"), warehouse.Id), warehouse);

            return Created(warehouse.Id, await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Warehouses.Added"));
        }

        //prepare model
        model = await _shippingModelFactory.PrepareWarehouseModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> EditWarehouse(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //try to get a warehouse with the specified id
        var warehouse = await _shippingService.GetWarehouseByIdAsync(id);
        if (warehouse == null)
            return NotFound("No warehouse found with the specified id");

        //prepare model
        var model = await _shippingModelFactory.PrepareWarehouseModelAsync(null, warehouse);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditWarehouse([FromBody] BaseQueryModel<WarehouseModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a warehouse with the specified id
        var warehouse = await _shippingService.GetWarehouseByIdAsync(model.Id);
        if (warehouse == null)
            return NotFound("No warehouse found with the specified id");

        if (ModelState.IsValid)
        {
            var address = await _addressService.GetAddressByIdAsync(warehouse.AddressId) ??
                new Address
                {
                    CreatedOnUtc = DateTime.UtcNow
                };
            address = model.Address.ToEntity(address);
            if (address.Id > 0)
                await _addressService.UpdateAddressAsync(address);
            else
                await _addressService.InsertAddressAsync(address);

            //fill entity from model
            warehouse = model.ToEntity(warehouse);

            warehouse.AddressId = address.Id;

            await _shippingService.UpdateWarehouseAsync(warehouse);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditWarehouse",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditWarehouse"), warehouse.Id), warehouse);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Warehouses.Updated"));
        }

        //prepare model
        model = await _shippingModelFactory.PrepareWarehouseModelAsync(model, warehouse, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> DeleteWarehouse(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //try to get a warehouse with the specified id
        var warehouse = await _shippingService.GetWarehouseByIdAsync(id);
        if (warehouse == null)
            return NotFound("No warehouse found with the specified id");

        await _shippingService.DeleteWarehouseAsync(warehouse);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteWarehouse",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteWarehouse"), warehouse.Id), warehouse);

        return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.warehouses.Deleted"));
    }

    #endregion

    #region Restrictions

    public virtual async Task<IActionResult> Restrictions()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _shippingModelFactory.PrepareShippingMethodRestrictionModelAsync(new ShippingMethodRestrictionModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RestrictionSave([FromBody] BaseQueryModel<ShippingMethodRestrictionModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var countries = await _countryService.GetAllCountriesAsync(showHidden: true);
        var shippingMethods = await _shippingService.GetAllShippingMethodsAsync();

        var form = queryModel.FormValues.ToNameValueCollection();
        foreach (var shippingMethod in shippingMethods)
        {
            var formKey = "restrict_" + shippingMethod.Id;
            var countryIdsToRestrict = !StringValues.IsNullOrEmpty(form[formKey])
                ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList()
                : new List<int>();

            foreach (var country in countries)
            {
                var restrict = countryIdsToRestrict.Contains(country.Id);
                var shippingMethodCountryMappings =
                    await _shippingService.GetShippingMethodCountryMappingAsync(shippingMethod.Id, country.Id);

                if (restrict)
                {
                    if (shippingMethodCountryMappings.Any())
                        continue;

                    await _shippingService.InsertShippingMethodCountryMappingAsync(new ShippingMethodCountryMapping { CountryId = country.Id, ShippingMethodId = shippingMethod.Id });
                    await _shippingService.UpdateShippingMethodAsync(shippingMethod);
                }
                else
                {
                    if (!shippingMethodCountryMappings.Any())
                        continue;

                    await _shippingService.DeleteShippingMethodCountryMappingAsync(shippingMethodCountryMappings.FirstOrDefault());
                    await _shippingService.UpdateShippingMethodAsync(shippingMethod);
                }
            }
        }

        return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Restrictions.Updated"));
    }

    #endregion
}