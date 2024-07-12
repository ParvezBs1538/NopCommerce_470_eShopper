using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Services.Attributes;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Orders;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/checkoutattribute/[action]")]
public partial class CheckoutAttributeApiController : BaseAdminApiController
{
    #region Fields

    private readonly CurrencySettings _currencySettings;
    private readonly ICheckoutAttributeModelFactory _checkoutAttributeModelFactory;
    private readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
    private readonly IAttributeService<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeService;
    private readonly ICurrencyService _currencyService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IMeasureService _measureService;
    private readonly IPermissionService _permissionService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;
    private readonly MeasureSettings _measureSettings;

    #endregion

    #region Ctor

    public CheckoutAttributeApiController(CurrencySettings currencySettings,
        ICheckoutAttributeModelFactory checkoutAttributeModelFactory,
        IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
        IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IMeasureService measureService,
        IPermissionService permissionService,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        MeasureSettings measureSettings)
    {
        _currencySettings = currencySettings;
        _checkoutAttributeModelFactory = checkoutAttributeModelFactory;
        _checkoutAttributeParser = checkoutAttributeParser;
        _checkoutAttributeService = checkoutAttributeService;
        _currencyService = currencyService;
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _measureService = measureService;
        _permissionService = permissionService;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
        _measureSettings = measureSettings;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateAttributeLocalesAsync(CheckoutAttribute checkoutAttribute, CheckoutAttributeModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(checkoutAttribute,
                x => x.Name,
                localized.Name,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(checkoutAttribute,
                x => x.TextPrompt,
                localized.TextPrompt,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(checkoutAttribute,
                x => x.DefaultValue,
                localized.DefaultValue,
                localized.LanguageId);
        }
    }

    protected virtual async Task UpdateValueLocalesAsync(CheckoutAttributeValue checkoutAttributeValue, CheckoutAttributeValueModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(checkoutAttributeValue,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    protected virtual async Task SaveStoreMappingsAsync(CheckoutAttribute checkoutAttribute, CheckoutAttributeModel model)
    {
        checkoutAttribute.LimitedToStores = model.SelectedStoreIds.Any();
        await _checkoutAttributeService.UpdateAttributeAsync(checkoutAttribute);

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(checkoutAttribute);
        var allStores = await _storeService.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                    await _storeMappingService.InsertStoreMappingAsync(checkoutAttribute, store.Id);
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

    protected virtual async Task SaveConditionAttributesAsync(CheckoutAttribute checkoutAttribute, CheckoutAttributeModel model)
    {
        string attributesXml = null;

        if (model.ConditionModel.EnableCondition)
        {
            var attribute = await _checkoutAttributeService.GetAttributeByIdAsync(model.ConditionModel.SelectedAttributeId);
            if (attribute != null)
            {
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    {
                        var selectedAttribute = model.ConditionModel.ConditionAttributes
                            .FirstOrDefault(x => x.Id == model.ConditionModel.SelectedAttributeId);
                        var selectedValue = selectedAttribute?.SelectedValueId;

                        //for conditions we should empty values save even when nothing is selected
                        //otherwise "attributesXml" will be empty
                        //hence we won't be able to find a selected attribute
                        attributesXml = _checkoutAttributeParser.AddAttribute(null, attribute, string.IsNullOrEmpty(selectedValue) ? string.Empty : selectedValue);
                    }
                    break;
                    case AttributeControlType.Checkboxes:
                    {
                        var selectedAttribute = model.ConditionModel.ConditionAttributes
                            .FirstOrDefault(x => x.Id == model.ConditionModel.SelectedAttributeId);
                        var selectedValues = selectedAttribute?.Values
                            .Where(x => x.Selected)
                            .Select(x => x.Value)
                            .ToList();

                        if (selectedValues?.Any() ?? false)
                            foreach (var value in selectedValues)
                                attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml, attribute, value);
                        else
                            attributesXml = _checkoutAttributeParser.AddAttribute(null, attribute, string.Empty);
                    }
                    break;
                    case AttributeControlType.ReadonlyCheckboxes:
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.FileUpload:
                    default:
                        //these attribute types are not supported as conditions
                        break;
                }
            }
        }

        checkoutAttribute.ConditionAttributeXml = attributesXml;
    }

    #endregion

    #region Checkout attributes

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _checkoutAttributeModelFactory.PrepareCheckoutAttributeSearchModelAsync(new CheckoutAttributeSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<CheckoutAttributeSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _checkoutAttributeModelFactory.PrepareCheckoutAttributeListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _checkoutAttributeModelFactory.PrepareCheckoutAttributeModelAsync(new CheckoutAttributeModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<CheckoutAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var checkoutAttribute = model.ToEntity<CheckoutAttribute>();
            await _checkoutAttributeService.InsertAttributeAsync(checkoutAttribute);

            //locales
            await UpdateAttributeLocalesAsync(checkoutAttribute, model);

            //stores
            await SaveStoreMappingsAsync(checkoutAttribute, model);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewCheckoutAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCheckoutAttribute"), checkoutAttribute.Name), checkoutAttribute);

            return Created(checkoutAttribute.Id, await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.CheckoutAttributes.Added"));
        }

        //prepare model
        model = await _checkoutAttributeModelFactory.PrepareCheckoutAttributeModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a checkout attribute with the specified id
        var checkoutAttribute = await _checkoutAttributeService.GetAttributeByIdAsync(id);
        if (checkoutAttribute == null)
            return NotFound("No checkout attribute found with the specified id");

        //prepare model
        var model = await _checkoutAttributeModelFactory.PrepareCheckoutAttributeModelAsync(null, checkoutAttribute);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<CheckoutAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a checkout attribute with the specified id
        var checkoutAttribute = await _checkoutAttributeService.GetAttributeByIdAsync(model.Id);
        if (checkoutAttribute == null)
            return NotFound("No checkout attribute found with the specified id");

        if (ModelState.IsValid)
        {
            checkoutAttribute = model.ToEntity(checkoutAttribute);
            await SaveConditionAttributesAsync(checkoutAttribute, model);
            await _checkoutAttributeService.UpdateAttributeAsync(checkoutAttribute);

            //locales
            await UpdateAttributeLocalesAsync(checkoutAttribute, model);

            //stores
            await SaveStoreMappingsAsync(checkoutAttribute, model);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditCheckoutAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCheckoutAttribute"), checkoutAttribute.Name), checkoutAttribute);

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.CheckoutAttributes.Updated"));
        }

        //prepare model
        model = await _checkoutAttributeModelFactory.PrepareCheckoutAttributeModelAsync(model, checkoutAttribute, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a checkout attribute with the specified id
        var checkoutAttribute = await _checkoutAttributeService.GetAttributeByIdAsync(id);
        if (checkoutAttribute == null)
            return NotFound("No checkout attribute found with the specified id");

        await _checkoutAttributeService.DeleteAttributeAsync(checkoutAttribute);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteCheckoutAttribute",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCheckoutAttribute"), checkoutAttribute.Name), checkoutAttribute);

        return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.CheckoutAttributes.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return NotFound();

        var checkoutAttributes = await _checkoutAttributeService.GetAttributeByIdsAsync(selectedIds.ToArray());
        await _checkoutAttributeService.DeleteAttributesAsync(checkoutAttributes);

        foreach (var checkoutAttribute in checkoutAttributes)
        {
            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteCheckoutAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCheckoutAttribute"), checkoutAttribute.Name), checkoutAttribute);
        }

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Checkout attribute values

    [HttpPost]
    public virtual async Task<IActionResult> ValueList([FromBody] BaseQueryModel<CheckoutAttributeValueSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a checkout attribute with the specified id
        var checkoutAttribute = await _checkoutAttributeService.GetAttributeByIdAsync(searchModel.CheckoutAttributeId);
        if (checkoutAttribute == null)
            return NotFound("No checkout attribute found with the specified id");

        //prepare model
        var model = await _checkoutAttributeModelFactory.PrepareCheckoutAttributeValueListModelAsync(searchModel, checkoutAttribute);

        return OkWrap(model);
    }

    [HttpGet("{checkoutAttributeId}")]
    public virtual async Task<IActionResult> ValueCreatePopup(int checkoutAttributeId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a checkout attribute with the specified id
        var checkoutAttribute = await _checkoutAttributeService.GetAttributeByIdAsync(checkoutAttributeId);
        if (checkoutAttribute == null)
            return NotFound("No checkout attribute found with the specified id");

        //prepare model
        var model = await _checkoutAttributeModelFactory
            .PrepareCheckoutAttributeValueModelAsync(new CheckoutAttributeValueModel(), checkoutAttribute, null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ValueCreatePopup([FromBody] BaseQueryModel<CheckoutAttributeValueModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a checkout attribute with the specified id
        var checkoutAttribute = await _checkoutAttributeService.GetAttributeByIdAsync(model.AttributeId);
        if (checkoutAttribute == null)
            return NotFound("No checkout attribute found with the specified id");

        model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
        model.BaseWeightIn = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)).Name;

        if (checkoutAttribute.AttributeControlType == AttributeControlType.ColorSquares)
        {
            //ensure valid color is chosen/entered
            if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                ModelState.AddModelError(string.Empty, "Color is required");

            try
            {
                //ensure color is valid (can be instantiated)
                System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
            }
            catch (Exception exc)
            {
                ModelState.AddModelError(string.Empty, exc.Message);
            }
        }

        if (ModelState.IsValid)
        {
            var checkoutAttributeValue = model.ToEntity<CheckoutAttributeValue>();
            await _checkoutAttributeService.InsertAttributeValueAsync(checkoutAttributeValue);

            await UpdateValueLocalesAsync(checkoutAttributeValue, model);

            return OkWrap(model);
        }

        //prepare model
        model = await _checkoutAttributeModelFactory.PrepareCheckoutAttributeValueModelAsync(model, checkoutAttribute, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> ValueEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a checkout attribute value with the specified id
        var checkoutAttributeValue = await _checkoutAttributeService.GetAttributeValueByIdAsync(id);
        if (checkoutAttributeValue == null)
            return NotFound("No checkout attribute value found with the specified id");

        //try to get a checkout attribute with the specified id
        var checkoutAttribute = await _checkoutAttributeService.GetAttributeByIdAsync(checkoutAttributeValue.AttributeId);
        if (checkoutAttribute == null)
            return NotFound("No checkout attribute found with the specified id");

        //prepare model
        var model = await _checkoutAttributeModelFactory.PrepareCheckoutAttributeValueModelAsync(null, checkoutAttribute, checkoutAttributeValue);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ValueEditPopup([FromBody] BaseQueryModel<CheckoutAttributeValueModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a checkout attribute value with the specified id
        var checkoutAttributeValue = await _checkoutAttributeService.GetAttributeValueByIdAsync(model.Id);
        if (checkoutAttributeValue == null)
            return NotFound("No checkout attribute value found with the specified id");

        //try to get a checkout attribute with the specified id
        var checkoutAttribute = await _checkoutAttributeService.GetAttributeByIdAsync(checkoutAttributeValue.AttributeId);
        if (checkoutAttribute == null)
            return NotFound("No checkout attribute found with the specified id");

        model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
        model.BaseWeightIn = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)).Name;

        if (checkoutAttribute.AttributeControlType == AttributeControlType.ColorSquares)
        {
            //ensure valid color is chosen/entered
            if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                ModelState.AddModelError(string.Empty, "Color is required");

            try
            {
                //ensure color is valid (can be instantiated)
                System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
            }
            catch (Exception exc)
            {
                ModelState.AddModelError(string.Empty, exc.Message);
            }
        }

        if (ModelState.IsValid)
        {
            checkoutAttributeValue = model.ToEntity(checkoutAttributeValue);
            await _checkoutAttributeService.UpdateAttributeValueAsync(checkoutAttributeValue);

            await UpdateValueLocalesAsync(checkoutAttributeValue, model);

            return OkWrap(model);
        }

        //prepare model
        model = await _checkoutAttributeModelFactory.PrepareCheckoutAttributeValueModelAsync(model, checkoutAttribute, checkoutAttributeValue, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ValueDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a checkout attribute value with the specified id
        var checkoutAttributeValue = await _checkoutAttributeService.GetAttributeValueByIdAsync(id);
        if (checkoutAttributeValue == null)
            return NotFound("No checkout attribute value found with the specified id");

        await _checkoutAttributeService.DeleteAttributeValueAsync(checkoutAttributeValue);

        return Ok(defaultMessage: true);
    }

    #endregion
}