using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Infrastructure;
using NopStation.Plugin.Misc.QuoteCart.Services;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;

public partial class QuoteFormModelFactory : IQuoteFormModelFactory
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IFormAttributeService _formAttributeService;
    private readonly IFormAttributeParser _formAttributeParser;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly IQuoteFormService _quoteFormService;
    private readonly ILocalizedModelFactory _localizedModelFactory;
    private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IEmailAccountService _emailAccountService;

    #endregion

    #region Ctor

    public QuoteFormModelFactory(
        ILocalizationService localizationService,
        IFormAttributeService formAttributeService,
        IFormAttributeParser formAttributeParser,
        IBaseAdminModelFactory baseAdminModelFactory,
        IQuoteFormService quoteCartFormService,
        ILocalizedModelFactory localizedModelFactory,
        IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        ILocalizedEntityService localizedEntityService,
        IEmailAccountService emailAccountService)
    {
        _localizationService = localizationService;
        _formAttributeService = formAttributeService;
        _formAttributeParser = formAttributeParser;
        _baseAdminModelFactory = baseAdminModelFactory;
        _quoteFormService = quoteCartFormService;
        _localizedModelFactory = localizedModelFactory;
        _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
        _localizedEntityService = localizedEntityService;
        _emailAccountService = emailAccountService;
    }

    #endregion

    #region Utilities

    protected async Task PrepareActiveOptionsAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        ArgumentNullException.ThrowIfNull(items);

        items.Add(new (await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.List.SearchActive.Active"), "1"));

        items.Add(new (await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.List.SearchActive.Inactive"), "2"));

        if (withSpecialDefaultItem)
            items.Insert(0, new (await _localizationService.GetResourceAsync("Admin.Common.All"), "0"));
    }

    #endregion

    #region Methods

    #region Quote form

    public async Task<QuoteFormModel> PrepareQuoteFormModelAsync(QuoteFormModel model, QuoteForm form, bool excludeProperties = false)
    {
        Func<QuoteFormLocalizedModel, int, Task> localizedModelConfiguration = null;
        var emailAccounts = await _emailAccountService.GetAllEmailAccountsAsync();
        if (form != null)
        {
            if (model == null)
            {
                model = form.ToModel<QuoteFormModel>();

                if (excludeProperties)
                    return model;

                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Title = await _localizationService.GetLocalizedAsync(form, entity => entity.Title, languageId, false, false);
                    locale.Info = await _localizationService.GetLocalizedAsync(form, entity => entity.Info, languageId, false, false);
                    locale.SubmitButtonText = await _localizationService.GetLocalizedAsync(form, entity => entity.SubmitButtonText, languageId, false, false);
                    locale.ShowTermsAndConditionsCheckbox = await _localizationService.GetLocalizedAsync(form, entity => entity.ShowTermsAndConditionsCheckbox, languageId, false, false);
                    locale.TermsAndConditions = await _localizationService.GetLocalizedAsync(form, entity => entity.TermsAndConditions, languageId, false, false);
                };
            }
        }

        if (excludeProperties)
            return model;

        model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

        await _baseAdminModelFactory.PrepareEmailAccountsAsync(model.AvailableEmailAccounts, false);

        await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, form, excludeProperties);

        model.FormAttributeMappingSearchModel = await PrepareFormAttributeMappingSearchModelAsync(new (), form);

        return model;
    }

    public virtual async Task<QuoteFormSearchModel> PrepareFormSearchModelAsync(QuoteFormSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        await PrepareActiveOptionsAsync(searchModel.AvailableActiveOptions, true);

        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        return searchModel;
    }

    public async Task<QuoteFormListModel> PrepareFormListModelAsync(QuoteFormSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var quoteForms = await _quoteFormService.GetAllQuoteFormsAsync(
            storeId: searchModel.SearchStoreId,
            active: null,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new QuoteFormListModel().PrepareToGridAsync(searchModel, quoteForms, () =>
        {
            return quoteForms.SelectAwait(async menu =>
            {
                return await PrepareQuoteFormModelAsync(null, menu, true);
            });
        });

        return model;
    }

    public async Task SaveStoreMappingsAsync(QuoteForm form, QuoteFormModel model)
    {
        form.LimitedToStores = model.SelectedStoreIds.Any();

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(form);
        var allStores = await _storeService.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                    await _storeMappingService.InsertStoreMappingAsync(form, store.Id);
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

    public async Task UpdateLocalesAsync(QuoteForm form, QuoteFormModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(form,
                    x => x.Title,
                    localized.Title,
                    localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(form,
                    x => x.Info,
                    localized.Info,
                    localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(form,
                    x => x.SubmitButtonText,
                    localized.SubmitButtonText,
                    localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(form,
                    x => x.ShowTermsAndConditionsCheckbox,
                    localized.ShowTermsAndConditionsCheckbox,
                    localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(form,
                    x => x.TermsAndConditions,
                    localized.TermsAndConditions,
                    localized.LanguageId);
        }
    }

    #endregion

    #region Form attribute mappings

    public virtual async Task<FormAttributeMappingSearchModel> PrepareFormAttributeMappingSearchModelAsync(FormAttributeMappingSearchModel searchModel, QuoteForm form)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.FormId = form?.Id ?? 0;

        searchModel.SetGridPageSize();

        return await Task.FromResult(searchModel);
    }

    public virtual async Task<FormAttributeMappingListModel> PrepareFormAttributeMappingListModelAsync(FormAttributeMappingSearchModel searchModel,
        QuoteForm quoteForm)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        ArgumentNullException.ThrowIfNull(quoteForm);

        //get form attribute mappings
        var formAttributeMappings = (await _formAttributeService
            .GetFormAttributeMappingsByQuoteFormIdAsync(quoteForm.Id)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new FormAttributeMappingListModel().PrepareToGridAsync(searchModel, formAttributeMappings, () =>
        {
            return formAttributeMappings.SelectAwait(async attributeMapping =>
            {
                //fill in model values from the entity
                var formAttributeMappingModel = attributeMapping.ToModel<FormAttributeMappingModel>();

                //fill in additional values (not existing in the entity)
                formAttributeMappingModel.ConditionString = string.Empty;

                formAttributeMappingModel.ValidationRulesString = await PrepareFormAttributeMappingValidationRulesStringAsync(attributeMapping);
                formAttributeMappingModel.FormAttribute = (await _formAttributeService.GetFormAttributeByIdAsync(attributeMapping.FormAttributeId))?.Name;
                formAttributeMappingModel.AttributeControlType = await _localizationService.GetLocalizedEnumAsync(attributeMapping.AttributeControlType);
                var conditionAttribute = (await _formAttributeParser
                    .ParseFormAttributeMappingsAsync(attributeMapping.ConditionAttributeXml))
                    .FirstOrDefault();
                if (conditionAttribute == null)
                    return formAttributeMappingModel;

                var conditionValue = (await _formAttributeParser
                    .ParseFormAttributeValuesAsync(attributeMapping.ConditionAttributeXml))
                    .FirstOrDefault();
                if (conditionValue != null)
                    formAttributeMappingModel.ConditionString =
                        $"{WebUtility.HtmlEncode((await _formAttributeService.GetFormAttributeByIdAsync(conditionAttribute.FormAttributeId)).Name)}: {WebUtility.HtmlEncode(conditionValue.Name)}";

                return formAttributeMappingModel;
            });
        });

        return model;
    }

    public virtual async Task<FormAttributeMappingModel> PrepareFormAttributeMappingModelAsync(FormAttributeMappingModel model,
        QuoteForm quoteForm, FormAttributeMapping formAttributeMapping, bool excludeProperties = false)
    {
        Func<FormAttributeMappingLocalizedModel, int, Task> localizedModelConfiguration = null;

        ArgumentNullException.ThrowIfNull(quoteForm);

        if (formAttributeMapping != null)
        {
            //fill in model values from the entity
            model ??= new FormAttributeMappingModel
            {
                Id = formAttributeMapping.Id,
                AttributeControlTypeId = formAttributeMapping.AttributeControlTypeId
            };

            model.FormAttribute = (await _formAttributeService.GetFormAttributeByIdAsync(formAttributeMapping.FormAttributeId)).Name;
            model.AttributeControlType = await _localizationService.GetLocalizedEnumAsync(formAttributeMapping.AttributeControlType);

            if (!excludeProperties)
            {
                model.FormAttributeId = formAttributeMapping.FormAttributeId;
                model.TextPrompt = formAttributeMapping.TextPrompt;
                model.IsRequired = formAttributeMapping.IsRequired;
                model.AttributeControlTypeId = formAttributeMapping.AttributeControlTypeId;
                model.DisplayOrder = formAttributeMapping.DisplayOrder;
                model.ValidationMinLength = formAttributeMapping.ValidationMinLength;
                model.ValidationMaxLength = formAttributeMapping.ValidationMaxLength;
                model.ValidationMinDate = formAttributeMapping.ValidationMinDate;
                model.DefaultDate = formAttributeMapping.DefaultDate;
                model.ValidationMaxDate = formAttributeMapping.ValidationMaxDate;
                model.ValidationFileAllowedExtensions = formAttributeMapping.ValidationFileAllowedExtensions;
                model.ValidationFileMaximumSize = formAttributeMapping.ValidationFileMaximumSize;
                model.DefaultValue = formAttributeMapping.DefaultValue;
            }

            //prepare condition attributes model
            model.ConditionAllowed = true;
            await PrepareFormAttributeConditionModelAsync(model.ConditionModel, formAttributeMapping);

            //define localized model configuration action
            localizedModelConfiguration = async (locale, languageId) =>
            {
                locale.TextPrompt = await _localizationService.GetLocalizedAsync(formAttributeMapping, entity => entity.TextPrompt, languageId, false, false);
                locale.DefaultValue = await _localizationService.GetLocalizedAsync(formAttributeMapping, entity => entity.DefaultValue, languageId, false, false);
            };

            //prepare nested search model
            PrepareFormAttributeValueSearchModel(model.FormAttributeValueSearchModel, formAttributeMapping);
        }

        model.FormId = quoteForm.Id;

        //prepare localized models
        if (!excludeProperties)
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

        //prepare available form attributes
        model.AvailableFormAttributes = (await _formAttributeService.GetAllFormAttributesAsync()).Select(formAttribute => new SelectListItem
        {
            Text = formAttribute.Name,
            Value = formAttribute.Id.ToString()
        }).ToList();

        return model;
    }

    public virtual async Task<FormAttributeValueListModel> PrepareFormAttributeValueListModelAsync(FormAttributeValueSearchModel searchModel,
        FormAttributeMapping formAttributeMapping)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        ArgumentNullException.ThrowIfNull(formAttributeMapping);

        //get form attribute values
        var formAttributeValues = (await _formAttributeService
            .GetFormAttributeValuesAsync(formAttributeMapping.Id)).ToPagedList(searchModel);

        //prepare list model
        var model = new FormAttributeValueListModel().PrepareToGrid(searchModel, formAttributeValues, () =>
        {
            return formAttributeValues.Select(value =>
            {
                //fill in model values from the entity
                var productAttributeValueModel = value.ToModel<FormAttributeValueModel>();
                productAttributeValueModel.Name = formAttributeMapping.AttributeControlType != AttributeControlType.ColorSquares
                    ? value.Name : $"{value.Name} - {value.ColorSquaresRgb}";

                return productAttributeValueModel;
            });
        });

        return model;
    }

    public virtual async Task<FormAttributeValueModel> PrepareFormAttributeValueModelAsync(FormAttributeValueModel model,
        FormAttributeMapping formAttributeMapping, FormAttributeValue formAttributeValue, bool excludeProperties = false)
    {
        ArgumentNullException.ThrowIfNull(formAttributeMapping);

        Func<FormAttributeValueLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (formAttributeValue != null)
        {
            //fill in model values from the entity
            model ??= new FormAttributeValueModel
            {
                FormAttributeMappingId = formAttributeValue.FormAttributeMappingId,
                Name = formAttributeValue.Name,
                ColorSquaresRgb = formAttributeValue.ColorSquaresRgb,
                DisplayColorSquaresRgb = formAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares,
                ImageSquaresPictureId = formAttributeValue.ImageSquaresPictureId,
                DisplayImageSquaresPicture = formAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares,
                IsPreSelected = formAttributeValue.IsPreSelected,
                DisplayOrder = formAttributeValue.DisplayOrder
            };

            //define localized model configuration action
            localizedModelConfiguration = async (locale, languageId) =>
            {
                locale.Name = await _localizationService.GetLocalizedAsync(formAttributeValue, entity => entity.Name, languageId, false, false);
            };
        }

        model.FormAttributeMappingId = formAttributeMapping.Id;
        model.DisplayColorSquaresRgb = formAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares;
        model.DisplayImageSquaresPicture = formAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares;

        //prepare localized models
        if (!excludeProperties)
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

        return model;
    }

    #endregion

    #endregion

    #region Utilities

    protected virtual async Task<string> PrepareFormAttributeMappingValidationRulesStringAsync(FormAttributeMapping attributeMapping)
    {
        if (!attributeMapping.ValidationRulesAllowed())
            return string.Empty;

        var validationRules = new StringBuilder(string.Empty);

        if (attributeMapping.AttributeControlType == AttributeControlType.Datepicker)
        {
            if (attributeMapping.DefaultDate.HasValue)
                validationRules.AppendFormat("{0}: {1}<br />",
                    await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.DefaultDate"),
                    attributeMapping.DefaultDate.Value.ToString("D"));

            if (attributeMapping.ValidationMinDate.HasValue)
                validationRules.AppendFormat("{0}: {1}<br />",
                    await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MinDate"),
                    attributeMapping.ValidationMinDate.Value.ToString("D"));

            if (attributeMapping.ValidationMaxDate.HasValue)
                validationRules.AppendFormat("{0}: {1}<br />",
                    await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MaxDate"),
                    attributeMapping.ValidationMaxDate.Value.ToString("D"));
        }
        else
        {
            if (attributeMapping.ValidationMinLength.HasValue)
                validationRules.AppendFormat("{0}: {1}<br />",
                    await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MinLength"),
                    attributeMapping.ValidationMinLength);

            if (attributeMapping.ValidationMaxLength.HasValue)
                validationRules.AppendFormat("{0}: {1}<br />",
                    await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MaxLength"),
                    attributeMapping.ValidationMaxLength);

            if (!string.IsNullOrEmpty(attributeMapping.ValidationFileAllowedExtensions))
                validationRules.AppendFormat("{0}: {1}<br />",
                    await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.FileAllowedExtensions"),
                    WebUtility.HtmlEncode(attributeMapping.ValidationFileAllowedExtensions));

            if (attributeMapping.ValidationFileMaximumSize.HasValue)
                validationRules.AppendFormat("{0}: {1}<br />",
                    await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.FileMaximumSize"),
                    attributeMapping.ValidationFileMaximumSize);

            if (!string.IsNullOrEmpty(attributeMapping.DefaultValue))
                validationRules.AppendFormat("{0}: {1}<br />",
                    await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.DefaultValue"),
                    WebUtility.HtmlEncode(attributeMapping.DefaultValue));
        }

        return validationRules.ToString();
    }

    protected virtual async Task PrepareFormAttributeConditionModelAsync(FormAttributeConditionModel model,
        FormAttributeMapping formAttributeMapping)
    {
        ArgumentNullException.ThrowIfNull(model);

        ArgumentNullException.ThrowIfNull(formAttributeMapping);

        model.FormAttributeMappingId = formAttributeMapping.Id;
        model.EnableCondition = !string.IsNullOrEmpty(formAttributeMapping.ConditionAttributeXml);

        //pre-select attribute and values
        var selectedPva = (await _formAttributeParser
            .ParseFormAttributeMappingsAsync(formAttributeMapping.ConditionAttributeXml))
            .FirstOrDefault();

        var attributes = (await _formAttributeService.GetFormAttributeMappingsByQuoteFormIdAsync(formAttributeMapping.QuoteFormId))
            //ignore non-combinable attributes (should have selectable values)
            .Where(x => x.CanBeUsedAsCondition())
            //ignore this attribute (it cannot depend on itself)
            .Where(x => x.Id != formAttributeMapping.Id)
            .ToList();

        foreach (var attribute in attributes)
        {
            var attributeModel = new FormAttributeConditionModel.FormAttributeModel
            {
                Id = attribute.Id,
                FormAttributeId = attribute.FormAttributeId,
                Name = (await _formAttributeService.GetFormAttributeByIdAsync(attribute.FormAttributeId))?.Name,
                TextPrompt = attribute.TextPrompt,
                IsRequired = attribute.IsRequired,
                AttributeControlType = attribute.AttributeControlType
            };

            if (attribute.ShouldHaveValues())
            {
                //values
                var attributeValues = await _formAttributeService.GetFormAttributeValuesAsync(attribute.Id);
                foreach (var attributeValue in attributeValues)
                {
                    var attributeValueModel = new FormAttributeConditionModel.FormAttributeValueModel
                    {
                        Id = attributeValue.Id,
                        Name = attributeValue.Name,
                        IsPreSelected = attributeValue.IsPreSelected
                    };
                    attributeModel.Values.Add(attributeValueModel);
                }

                //pre-select attribute and value
                if (selectedPva != null && attribute.Id == selectedPva.Id)
                {
                    //attribute
                    model.SelectedFormAttributeId = selectedPva.Id;

                    //values
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            if (!string.IsNullOrEmpty(formAttributeMapping.ConditionAttributeXml))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues =
                                    await _formAttributeParser.ParseFormAttributeValuesAsync(formAttributeMapping
                                        .ConditionAttributeXml);
                                foreach (var attributeValue in selectedValues)
                                    foreach (var item in attributeModel.Values)
                                        if (attributeValue.Id == item.Id)
                                            item.IsPreSelected = true;
                            }

                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.FileUpload:
                        default:
                            //these attribute types are supported as conditions
                            break;
                    }
                }
            }

            model.FormAttributes.Add(attributeModel);
        }
    }

    protected virtual FormAttributeMappingSearchModel PrepareFormAttributeMappingSearchModel(FormAttributeMappingSearchModel searchModel,
        QuoteForm quoteForm)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        ArgumentNullException.ThrowIfNull(quoteForm);

        searchModel.FormId = quoteForm.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    protected virtual FormAttributeValueSearchModel PrepareFormAttributeValueSearchModel(FormAttributeValueSearchModel searchModel,
        FormAttributeMapping formAttributeMapping)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        ArgumentNullException.ThrowIfNull(formAttributeMapping);

        searchModel.FormAttributeMappingId = formAttributeMapping.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    #endregion
}
