using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Directory;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/currency/[action]")]
public partial class CurrencyApiController : BaseAdminApiController
{
    #region Fields

    private readonly CurrencySettings _currencySettings;
    private readonly ICurrencyModelFactory _currencyModelFactory;
    private readonly ICurrencyService _currencyService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;
    private readonly ILogger _logger;

    #endregion

    #region Ctor

    public CurrencyApiController(CurrencySettings currencySettings,
        ICurrencyModelFactory currencyModelFactory,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        ILogger logger)
    {
        _currencySettings = currencySettings;
        _currencyModelFactory = currencyModelFactory;
        _currencyService = currencyService;
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
        _logger = logger;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(Currency currency, CurrencyModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(currency, x => x.Name, localized.Name, localized.LanguageId);
        }
    }

    protected virtual async Task SaveStoreMappingsAsync(Currency currency, CurrencyModel model)
    {
        currency.LimitedToStores = model.SelectedStoreIds.Any();
        await _currencyService.UpdateCurrencyAsync(currency);

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(currency);
        var allStores = await _storeService.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                    await _storeMappingService.InsertStoreMappingAsync(currency, store.Id);
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

    #endregion

    #region Methods

    [HttpGet("{liveRates}")]
    public virtual async Task<IActionResult> List(bool liveRates = false)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrencies))
            return AdminApiAccessDenied();

        var model = new CurrencySearchModel();

        try
        {
            //prepare model
            model = await _currencyModelFactory.PrepareCurrencySearchModelAsync(model, liveRates);

            return OkWrap(model);
        }
        catch (Exception e)
        {
            await _logger.ErrorAsync(e.Message, e);

            return BadRequestWrap(model, null, new List<string> { e.Message });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<CurrencySearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrencies))
            return AdminApiAccessDenied();

        var model = queryModel.Data;

        _currencySettings.ActiveExchangeRateProviderSystemName = model.ExchangeRateProviderModel.ExchangeRateProvider;
        _currencySettings.AutoUpdateEnabled = model.ExchangeRateProviderModel.AutoUpdateEnabled;
        await _settingService.SaveSettingAsync(_currencySettings);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ListGrid([FromBody] BaseQueryModel<CurrencySearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrencies))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _currencyModelFactory.PrepareCurrencyListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ApplyRates([FromBody] BaseQueryModel<IEnumerable<CurrencyExchangeRateModel>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrencies))
            return AdminApiAccessDenied();

        var rateModels = queryModel.Data;
        foreach (var rate in rateModels)
        {
            var currency = await _currencyService.GetCurrencyByCodeAsync(rate.CurrencyCode);
            if (currency == null)
                continue;

            currency.Rate = rate.Rate;
            currency.UpdatedOnUtc = DateTime.UtcNow;
            await _currencyService.UpdateCurrencyAsync(currency);
        }

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> MarkAsPrimaryExchangeRateCurrency(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrencies))
            return AdminApiAccessDenied();

        _currencySettings.PrimaryExchangeRateCurrencyId = id;
        await _settingService.SaveSettingAsync(_currencySettings);

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> MarkAsPrimaryStoreCurrency(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrencies))
            return AdminApiAccessDenied();

        _currencySettings.PrimaryStoreCurrencyId = id;
        await _settingService.SaveSettingAsync(_currencySettings);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Create / Edit / Delete

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrencies))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _currencyModelFactory.PrepareCurrencyModelAsync(new CurrencyModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<CurrencyModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrencies))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var currency = model.ToEntity<Currency>();
            currency.CreatedOnUtc = DateTime.UtcNow;
            currency.UpdatedOnUtc = DateTime.UtcNow;
            await _currencyService.InsertCurrencyAsync(currency);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewCurrency",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCurrency"), currency.Id), currency);

            //locales
            await UpdateLocalesAsync(currency, model);

            //stores
            await SaveStoreMappingsAsync(currency, model);

            return Created(currency.Id, await _localizationService.GetResourceAsync("Admin.Configuration.Currencies.Added"));
        }

        //prepare model
        model = await _currencyModelFactory.PrepareCurrencyModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrencies))
            return AdminApiAccessDenied();

        //try to get a currency with the specified id
        var currency = await _currencyService.GetCurrencyByIdAsync(id);
        if (currency == null)
            return NotFound("No currency found with the specified id");

        //prepare model
        var model = await _currencyModelFactory.PrepareCurrencyModelAsync(null, currency);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<CurrencyModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrencies))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a currency with the specified id
        var currency = await _currencyService.GetCurrencyByIdAsync(model.Id);
        if (currency == null)
            return NotFound("No currency found with the specified id");

        if (ModelState.IsValid)
        {
            //ensure we have at least one published currency
            var allCurrencies = await _currencyService.GetAllCurrenciesAsync();
            if (allCurrencies.Count == 1 && allCurrencies[0].Id == currency.Id && !model.Published)
            {
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Configuration.Currencies.PublishedCurrencyRequired"));
            }

            currency = model.ToEntity(currency);
            currency.UpdatedOnUtc = DateTime.UtcNow;
            await _currencyService.UpdateCurrencyAsync(currency);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditCurrency",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCurrency"), currency.Id), currency);

            //locales
            await UpdateLocalesAsync(currency, model);

            //stores
            await SaveStoreMappingsAsync(currency, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Currencies.Updated"));
        }

        //prepare model
        model = await _currencyModelFactory.PrepareCurrencyModelAsync(model, currency, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrencies))
            return AdminApiAccessDenied();

        //try to get a currency with the specified id
        var currency = await _currencyService.GetCurrencyByIdAsync(id);
        if (currency == null)
            return NotFound("No currency found with the specified id");

        try
        {
            if (currency.Id == _currencySettings.PrimaryStoreCurrencyId)
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Configuration.Currencies.CantDeletePrimary"));

            if (currency.Id == _currencySettings.PrimaryExchangeRateCurrencyId)
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Configuration.Currencies.CantDeleteExchange"));

            //ensure we have at least one published currency
            var allCurrencies = await _currencyService.GetAllCurrenciesAsync();
            if (allCurrencies.Count == 1 && allCurrencies[0].Id == currency.Id)
            {
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Configuration.Currencies.PublishedCurrencyRequired"));
            }

            await _currencyService.DeleteCurrencyAsync(currency);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteCurrency",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCurrency"), currency.Id), currency);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Currencies.Deleted"));
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc);
            return BadRequest(exc.Message);
        }
    }

    #endregion
}